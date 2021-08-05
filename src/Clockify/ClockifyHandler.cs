using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Clockify.Fill;
using Bot.Clockify.Reports;
using Bot.States;
using Bot.Supports;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Bot.Clockify
{
    public class ClockifyHandler : IBotHandler
    {
        private readonly EntryFillDialog _fillDialog;
        private readonly ReportDialog _reportDialog;
        private readonly StopReminderDialog _stopReminderDialog;
        private readonly ClockifySetupDialog _clockifySetupDialog;
        private readonly IClockifyService _clockifyService;
        private readonly DialogSet _dialogSet;
        private readonly IStatePropertyAccessor<DialogState> _dialogState;
        private readonly LuisRecognizerProxy _luisRecognizer;

        public ClockifyHandler(EntryFillDialog fillDialog, ReportDialog reportDialog,
            StopReminderDialog stopReminderDialog, IClockifyService clockifyService,
            ConversationState conversationState, ClockifySetupDialog clockifySetupDialog,
            LuisRecognizerProxy luisRecognizer)
        {
            _dialogState = conversationState.CreateProperty<DialogState>("ClockifyDialogState");
            _fillDialog = fillDialog;
            _reportDialog = reportDialog;
            _stopReminderDialog = stopReminderDialog;
            _clockifyService = clockifyService;
            _clockifySetupDialog = clockifySetupDialog;
            _luisRecognizer = luisRecognizer;
            _dialogSet = new DialogSet(_dialogState)
                .Add(_fillDialog)
                .Add(_stopReminderDialog)
                .Add(_reportDialog)
                .Add(_clockifySetupDialog);
        }

        public async Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken,
            UserProfile userProfile)
        {
            var dialogContext = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);
            bool anyActiveDialog = dialogContext.ActiveDialog != null;
            if (anyActiveDialog)
            {
                await dialogContext.ContinueDialogAsync(cancellationToken);
                return true;
            }

            if (await RunClockifySetupIfNeeded(turnContext, cancellationToken, userProfile)) return true;
            
            var (topIntent, entities) =
                await _luisRecognizer.RecognizeAsyncIntent(turnContext, cancellationToken);

            // TODO add integration test and put this condition inside the appropriate switch
            // Sometimes the intent is not recognized properly
            if (entities.datetime != null && entities.WorkedEntity != null)
            {
                await dialogContext.BeginDialogAsync(_fillDialog.Id, entities, cancellationToken);
                return true;
            }

            switch (topIntent)
            {
                case TimeSurveyBotLuis.Intent.Thanks:
                {
                    const string message = "You're welcome ❤";
                    await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                    return true;
                }
                case TimeSurveyBotLuis.Intent.Insult:
                {
                    const string message = "Language, please...";
                    await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                    return true;
                }
                case TimeSurveyBotLuis.Intent.Report:
                    await dialogContext.BeginDialogAsync(_reportDialog.Id, entities, cancellationToken);
                    return true;
                case TimeSurveyBotLuis.Intent.Fill:
                    await dialogContext.BeginDialogAsync(_fillDialog.Id, entities, cancellationToken);
                    return true;
                case TimeSurveyBotLuis.Intent.FillAsYesterday:
                    // Unused
                    break;
                case TimeSurveyBotLuis.Intent.None:
                    break;
                case TimeSurveyBotLuis.Intent.Utilities_Help:
                {
                    const string message = "I can sure help you. This is what I can do:\n" +
                                           "- **reporting**: ask me to give you insight about a reporting period, and surprisingly enough I will! " +
                                           "For example, ask me *how much did I work last week?' and I'll give you all needed info\n\n" +
                                           "- **insertion**: feel like adding some entries? just tell me! For example, say to me *add 15 minutes on " +
                                           "r&d* and I will add it to today's time sheet.\n\n\n" +
                                           "Working with multiple workspaces? Don't worry, I got you covered";
                    await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                    return true;
                }
                case TimeSurveyBotLuis.Intent.Utilities_Stop:
                    await dialogContext.BeginDialogAsync(_stopReminderDialog.Id, entities, cancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(topIntent), topIntent, null);
            }

            return false;
        }
        
        private async Task<bool> RunClockifySetupIfNeeded(ITurnContext turnContext, CancellationToken cancellationToken,
            UserProfile userProfile)
        {
            if (userProfile.ClockifyToken == null)
            {
                await _clockifySetupDialog.RunAsync(turnContext, _dialogState, cancellationToken);
                return true;
            }

            try
            {
                await _clockifyService.GetCurrentUserAsync(userProfile.ClockifyToken);
            }
            catch (ErrorResponseException)
            {
                // TODO check if it is an UnauthorizedException
                await _clockifySetupDialog.RunAsync(turnContext, _dialogState, cancellationToken);
                return true;
            }

            return false;
        }
    }
}