using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Clockify.Fill;
using Bot.Clockify.Models;
using Bot.Clockify.Reports;
using Bot.Common;
using Bot.Data;
using Bot.States;
using Bot.Supports;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

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
        private readonly CommonRecognizer _recognizerProxy;
        private readonly ITokenRepository _tokenRepository;

        public ClockifyHandler(EntryFillDialog fillDialog, ReportDialog reportDialog,
            StopReminderDialog stopReminderDialog, IClockifyService clockifyService,
            ConversationState conversationState, ClockifySetupDialog clockifySetupDialog,
            CommonRecognizer recognizerProxy, ITokenRepository tokenRepository)
        {
            _dialogState = conversationState.CreateProperty<DialogState>("ClockifyDialogState");
            _fillDialog = fillDialog;
            _reportDialog = reportDialog;
            _stopReminderDialog = stopReminderDialog;
            _clockifyService = clockifyService;
            _clockifySetupDialog = clockifySetupDialog;
            _recognizerProxy = recognizerProxy;
            _tokenRepository = tokenRepository;
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

            if (await RunClockifySetupIfNeeded(turnContext, cancellationToken, userProfile)) return true;

            var luisResult = await _recognizerProxy.RecognizeAsync<TimeSurveyBotLuis>(turnContext, cancellationToken);

            try
            {
                switch (luisResult.TopIntentWithMinScore())
                {
                    case TimeSurveyBotLuis.Intent.Report:
                        await dialogContext.BeginDialogAsync(_reportDialog.Id, luisResult.Entities._instance,
                            cancellationToken);
                        return true;
                    case TimeSurveyBotLuis.Intent.Fill:
                        await dialogContext.BeginDialogAsync(_fillDialog.Id, luisResult.Entities._instance,
                            cancellationToken);
                        return true;
                    case TimeSurveyBotLuis.Intent.FillAsYesterday:
                        return false;
                    case TimeSurveyBotLuis.Intent.Utilities_Stop:
                        await dialogContext.BeginDialogAsync(_stopReminderDialog.Id, luisResult.Entities._instance,
                            cancellationToken);
                        return true;
                    default:
                        return false;
                }
            }
            catch (UnauthorizedAccessException)
            {
                await dialogContext.BeginDialogAsync(_clockifySetupDialog.Id, _dialogState, cancellationToken);
                return true;
            }
        }

        public DialogSet GetDialogSet() => _dialogSet;

        private async Task<bool> RunClockifySetupIfNeeded(ITurnContext turnContext, CancellationToken cancellationToken,
            UserProfile userProfile)
        {
            if (userProfile.ClockifyTokenId == null && userProfile.ClockifyToken == null)
            {
                await _clockifySetupDialog.RunAsync(turnContext, _dialogState, cancellationToken);
                return true;
            }

            try
            {
                // TODO: it will be removed when only ClockifyTokenId will be used
                if (userProfile.ClockifyTokenId == null && userProfile.ClockifyToken != null)
                {
                    var tokenData = await _tokenRepository.WriteAsync(userProfile.ClockifyToken);
                    userProfile.ClockifyToken = null;
                    userProfile.ClockifyTokenId = tokenData.Id;
                }
                else
                {
                    // ClockifyTokenId can't be null.
                    if (userProfile.Email == null)
                    {
                        var tokenData = await _tokenRepository.ReadAsync(userProfile.ClockifyTokenId!);
                        UserDo user = await _clockifyService.GetCurrentUserAsync(tokenData.Value);
                        userProfile.ClockifyToken = null;
                        // This can be removed in future, it serves the purpose of aligning old users
                        string? fullName = user.Name;
                        if (fullName != null)
                        {
                            //TODO: this might be wrong, don't care
                            userProfile.FirstName = fullName.Split(" ")[0];
                            userProfile.LastName =
                                new string(fullName.Skip(userProfile.FirstName.Length + 1).ToArray());
                        }

                        userProfile.Email = user.Email;
                    }
                }

                return false;
            }
            catch (UnauthorizedAccessException)
            {
                await _clockifySetupDialog.RunAsync(turnContext, _dialogState, cancellationToken);
                return true;
            }
        }
    }
}