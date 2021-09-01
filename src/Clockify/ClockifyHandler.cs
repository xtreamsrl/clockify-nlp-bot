using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Clockify.Fill;
using Bot.Clockify.Reports;
using Bot.Data;
using Bot.States;
using Bot.Supports;
using Clockify.Net.Models.Users;
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
        private readonly ITokenRepository _tokenRepository;

        public ClockifyHandler(EntryFillDialog fillDialog, ReportDialog reportDialog,
            StopReminderDialog stopReminderDialog, IClockifyService clockifyService,
            ConversationState conversationState, ClockifySetupDialog clockifySetupDialog,
            LuisRecognizerProxy luisRecognizer, ITokenRepository tokenRepository)
        {
            _dialogState = conversationState.CreateProperty<DialogState>("ClockifyDialogState");
            _fillDialog = fillDialog;
            _reportDialog = reportDialog;
            _stopReminderDialog = stopReminderDialog;
            _clockifyService = clockifyService;
            _clockifySetupDialog = clockifySetupDialog;
            _luisRecognizer = luisRecognizer;
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

            var (topIntent, entities) =
                await _luisRecognizer.RecognizeAsyncIntent(turnContext, cancellationToken);

            switch (topIntent)
            {
                case TimeSurveyBotLuis.Intent.Report:
                    await dialogContext.BeginDialogAsync(_reportDialog.Id, entities, cancellationToken);
                    return true;
                case TimeSurveyBotLuis.Intent.Fill:
                    await dialogContext.BeginDialogAsync(_fillDialog.Id, entities, cancellationToken);
                    return true;
                case TimeSurveyBotLuis.Intent.FillAsYesterday:
                    return false;
                case TimeSurveyBotLuis.Intent.Utilities_Stop:
                    await dialogContext.BeginDialogAsync(_stopReminderDialog.Id, entities, cancellationToken);
                    return true;
                default: 
                    return false;
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
                // It will be removed when only ClockifyTokenId will be used
                if (userProfile.ClockifyTokenId == null && userProfile.ClockifyToken != null)
                {
                    await _clockifyService.GetCurrentUserAsync(userProfile.ClockifyToken);
                    var tokenData = await _tokenRepository.WriteAsync(userProfile.ClockifyToken);
                    userProfile.ClockifyToken = null;
                    userProfile.ClockifyTokenId = tokenData.Id;
                }
                else
                {
                    // ClockifyTokenId can't be null.
                    var tokenData = await _tokenRepository.ReadAsync(userProfile.ClockifyTokenId!);
                    CurrentUserDto user = await _clockifyService.GetCurrentUserAsync(tokenData.Value);
                    userProfile.ClockifyToken = null;
                    // This can be removed in future, it serves the purpose of aligning old users
                    if (user.Name != null)
                    {
                        userProfile.FirstName = user.Name.Split(" ")[0]; //TODO: this might be wrong, don't care
                        userProfile.LastName = user.Name.Skip(userProfile.FirstName.Length + 1).ToString();
                        userProfile.Email = user.Email;
                    }
                }

                return false;
            }
            catch (ErrorResponseException)
            {
                // TODO check if it is an UnauthorizedException
                // old login is invalid, asking again
                await _clockifySetupDialog.RunAsync(turnContext, _dialogState, cancellationToken);
                return true;
            }
        }
    }
}