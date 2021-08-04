using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Dialogs;
using Bot.States;
using Bot.Supports;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Bot.DIC
{
    public class DicHandler: IBotHandler
    {
        private readonly NextWeekRemoteWorkingDialog _nextWeekRemoteWorkingDialog;
        private readonly LongTermRemoteWorkingDialog _longTermRemoteWorkingDialog;
        private readonly TeamAvailabilityService _teamAvailabilityService;
        private readonly NotifyUsersDialog _notifyUsersDialog;
        private readonly IDipendentiInCloudService _dicService;
        private readonly DialogSet _dialogSet;
        private readonly DicSetupDialog _dicSetupDialog;
        private readonly IStatePropertyAccessor<DialogState> _dialogState;

        public DicHandler(NextWeekRemoteWorkingDialog nextWeekRemoteWorkingDialog,
            LongTermRemoteWorkingDialog longTermRemoteWorkingDialog, TeamAvailabilityService teamAvailabilityService,
            NotifyUsersDialog notifyUsersDialog,
            ConversationState conversationState, IDipendentiInCloudService dicService, DicSetupDialog dicSetupDialog)
        {
            _dicService = dicService;
            _dicSetupDialog = dicSetupDialog;
            _dialogState = conversationState.CreateProperty<DialogState>("DicDialogState");
            _nextWeekRemoteWorkingDialog = nextWeekRemoteWorkingDialog;
            _longTermRemoteWorkingDialog = longTermRemoteWorkingDialog;
            _teamAvailabilityService = teamAvailabilityService;
            _notifyUsersDialog = notifyUsersDialog;
            _dialogSet = new DialogSet(_dialogState)
                .Add(_nextWeekRemoteWorkingDialog)
                .Add(_longTermRemoteWorkingDialog)
                .Add(_notifyUsersDialog);
        }

        public async Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken, UserProfile userProfile)
        {
            var dialogContext = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);
            bool anyActiveDialog = dialogContext.ActiveDialog != null;
            if (anyActiveDialog)
            {
                await dialogContext.ContinueDialogAsync(cancellationToken);
                return true;
            }
            
            // TODO add dic setup to the dialog set
            if (await RunDICSetupIfNeeded(turnContext, cancellationToken, userProfile)) return true;

            var isMaintainer = false;
            if (userProfile.DicToken != null)
            {
                var currentEmployee = await _dicService.GetCurrentEmployeeAsync(userProfile.DicToken!);
                isMaintainer = currentEmployee.teams.Any(t => t.team.name == "Bot Maintainers");
            }

            switch (turnContext.Activity.Text.ToLower())
            {
                case "enter test mode" when !userProfile.Experimental:
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text("Ok sir, you will be getting experimental features"), cancellationToken);
                    userProfile.Experimental = true;
                    return true;
                case "exit test mode" when userProfile.Experimental:
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text("Ok sir, I'm cutting you out of tests"), cancellationToken);
                    userProfile.Experimental = false;
                    return true;
                case "next week remote" when userProfile.Experimental:
                    await dialogContext.BeginDialogAsync(_nextWeekRemoteWorkingDialog.Id,
                        cancellationToken: cancellationToken);
                    return true;
                case "set remote days" when userProfile.Experimental:
                    await dialogContext.BeginDialogAsync(_longTermRemoteWorkingDialog.Id,
                        cancellationToken: cancellationToken);
                    return true;
                case "my team" when userProfile.Experimental:
                {
                    var report = await _teamAvailabilityService.CreateAvailabilityReportAsync(userProfile);
                    await dialogContext.Context.SendActivityAsync(MessageFactory.Attachment(report), cancellationToken);
                    return true;
                }
                case "notify users" when isMaintainer:
                    await dialogContext.BeginDialogAsync(_notifyUsersDialog.Id, cancellationToken: cancellationToken);
                    return true;
                default:
                    return false;
            }
        }

        // ReSharper disable once InconsistentNaming
        private async Task<bool> RunDICSetupIfNeeded(ITurnContext turnContext, CancellationToken cancellationToken,
            UserProfile userProfile)
        {
            if (!userProfile.Experimental) return false;
            if (userProfile.DicToken == null)
            {
                await _dicSetupDialog.RunAsync(turnContext, _dialogState, cancellationToken);
                return true;
            }

            try
            {
                await _dicService.GetCurrentEmployeeAsync(userProfile.DicToken);
            }
            catch (ErrorResponseException)
            {
                await _dicSetupDialog.RunAsync(turnContext, _dialogState, cancellationToken);
                return true;
            }

            return false;
        }
    }
}