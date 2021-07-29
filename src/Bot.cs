using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Dialogs;
using Bot.Services;
using Bot.Services.Reports;
using Bot.States;
using Bot.Utils;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using RestSharp.Extensions;

namespace Bot
{
    public class Bot : ActivityHandler
    {
        private readonly ConversationState _conversationState;
        private readonly EntryFillDialog _fillDialog;
        private readonly LuisRecognizerProxy _luisRecognizer;
        private readonly ReportDialog _reportDialog;
        private readonly ClockifySetupDialog _clockifySetupDialog;
        private readonly StopReminderDialog _stopReminderDialog;
        private readonly IClockifyService _clockifyService;
        private readonly DicSetupDialog _dicSetupDialog;
        private readonly IDipendentiInCloudService _dicService;
        private readonly NextWeekRemoteWorkingDialog _nextWeekRemoteWorkingDialog;
        private readonly LongTermRemoteWorkingDialog _longTermRemoteWorkingDialog;
        private readonly TeamAvailabilityService _teamAvailabilityService;
        private readonly NotifyUsersDialog _notifyUsersDialog;


        private readonly UserState _userState;

        private readonly DialogSet _dialogSet;
        private readonly IStatePropertyAccessor<DialogState> _dialogState;

        public Bot(ConversationState conversationState, EntryFillDialog fillDialog,
            LuisRecognizerProxy luisRecognizer,
            ReportDialog reportDialog, ClockifySetupDialog clockifySetupDialog, UserState userState, 
            StopReminderDialog stopReminderDialog, IClockifyService clockifyService, 
            DicSetupDialog dicSetupDialog, IDipendentiInCloudService dicService, 
            NextWeekRemoteWorkingDialog nextWeekRemoteWorkingDialog, 
            LongTermRemoteWorkingDialog longTermRemoteWorkingDialog, TeamAvailabilityService teamAvailabilityService, 
            NotifyUsersDialog notifyUsersDialog)
        {
            _conversationState = conversationState;
            _dialogState = _conversationState.CreateProperty<DialogState>("DialogState");
            _fillDialog = fillDialog;
            _luisRecognizer = luisRecognizer;
            _reportDialog = reportDialog;
            _clockifySetupDialog = clockifySetupDialog;
            _userState = userState;
            _stopReminderDialog = stopReminderDialog;
            _clockifyService = clockifyService;
            _dicSetupDialog = dicSetupDialog;
            _dicService = dicService;
            _nextWeekRemoteWorkingDialog = nextWeekRemoteWorkingDialog;
            _longTermRemoteWorkingDialog = longTermRemoteWorkingDialog;
            _teamAvailabilityService = teamAvailabilityService;
            _notifyUsersDialog = notifyUsersDialog;
            _dialogSet = new DialogSet(_dialogState)
                .Add(_clockifySetupDialog)
                .Add(_fillDialog)
                .Add(_reportDialog)
                .Add(_nextWeekRemoteWorkingDialog)
                .Add(_longTermRemoteWorkingDialog)
                .Add(_notifyUsersDialog)
                .Add(_stopReminderDialog);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext,
            CancellationToken cancellationToken = new CancellationToken())
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var dialogContext = await _dialogSet.CreateContextAsync(turnContext, cancellationToken);
            var userProfile =
                await StaticUserProfileHelper.GetUserProfileAsync(_userState, turnContext, cancellationToken);
            userProfile.ConversationReference = turnContext.Activity.GetConversationReference();
            if (await RunDICSetupIfNeeded(turnContext, cancellationToken, userProfile)) return;
            if (await RunClockifySetupIfNeeded(turnContext, cancellationToken, userProfile)) return;

            bool anyActiveDialog = dialogContext.ActiveDialog != null;
            var isMaintainer = false;
            if (userProfile.DicToken != null)
            {
                var currentEmployee = await _dicService.GetCurrentEmployeeAsync(userProfile.DicToken!);
                isMaintainer = currentEmployee.teams.Any(t => t.team.name == "Bot Maintainers");
            }
            if (!anyActiveDialog)
            {
                var (topIntent, entities) =
                    await _luisRecognizer.RecognizeAsyncIntent(turnContext, cancellationToken);

                if (turnContext.Activity.Text.ToLower() == "enter test mode" && !userProfile.Experimental)
                {
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text("Ok sir, you will be getting experimental features"), cancellationToken);
                    userProfile.Experimental = true;
                    return;
                }
                
                if (turnContext.Activity.Text.ToLower() == "exit test mode" && userProfile.Experimental)
                {
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text("Ok sir, I'm cutting you out of tests"), cancellationToken);
                    userProfile.Experimental = false;
                    return;
                }

                if (turnContext.Activity.Text.ToLower() == "next week remote" && userProfile.Experimental)
                {
                    await dialogContext.BeginDialogAsync(_nextWeekRemoteWorkingDialog.Id, entities, cancellationToken);
                    return;
                }
                
                if (turnContext.Activity.Text.ToLower() == "set remote days" && userProfile.Experimental)
                {
                    await dialogContext.BeginDialogAsync(_longTermRemoteWorkingDialog.Id, entities, cancellationToken);
                    return;
                }
                
                if (turnContext.Activity.Text.ToLower() == "my team" && userProfile.Experimental)
                {
                    var report = await _teamAvailabilityService.CreateAvailabilityReportAsync(userProfile);
                    await dialogContext.Context.SendActivityAsync(MessageFactory.Attachment(report), cancellationToken);
                    return;
                }

                if (turnContext.Activity.Text.ToLower() == "notify users" && isMaintainer)
                {
                    await dialogContext.BeginDialogAsync(_notifyUsersDialog.Id, entities, cancellationToken);
                    return;
                }

                if (topIntent == TimeSurveyBotLuis.Intent.Utilities_Help)
                {
                    const string message = "I can sure help you. This is what I can do:\n" +
                                           "- **reporting**: ask me to give you insight about a reporting period, and surprisingly enough I will! " +
                                           "For example, ask me *how much did I work last week?' and I'll give you all needed info\n\n" +
                                           "- **insertion**: feel like adding some entries? just tell me! For example, say to me *add 15 minutes on " +
                                           "r&d* and I will add it to today's time sheet.\n\n\n" +
                                           "Working with multiple workspaces? Don't worry, I got you covered";
                    await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                    return;
                }


                bool intentHasBeenHandled = await IntentManager.HandleIntent(dialogContext, topIntent, cancellationToken, 
                    turnContext, entities, _fillDialog, _reportDialog, _stopReminderDialog);
                if (!intentHasBeenHandled)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                            "mmm, I don't know exactly how to respond to " +
                            "that 😔... if you're stuck, just ask me for help"),
                        cancellationToken);
                }
            }
            else
            {
                await dialogContext.ContinueDialogAsync(cancellationToken);
            }
        }

        private async Task<bool> RunClockifySetupIfNeeded(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken,
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
                await _clockifySetupDialog.RunAsync(turnContext, _dialogState, cancellationToken);
                return true;
            }

            return false;
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

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id == turnContext.Activity.Recipient.Id) continue;
                await turnContext.SendActivityAsync(MessageFactory.Text(
                    $"Welcome {member.Name}! I assume you're quite familiar with my skills already, " +
                    "but should you need any further info just ask for help 😉. I don't have any special requirement, " +
                    "you just need to be a Clockify user for mw to crunch my work"), cancellationToken);
            }
        }
    }
}