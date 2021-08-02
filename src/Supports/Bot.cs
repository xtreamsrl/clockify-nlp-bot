using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Dialogs;
using Bot.DIC;
using Bot.Services;
using Bot.Services.Reports;
using Bot.States;
using Bot.Utils;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Bot.Supports
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
        private readonly DicHandler _dicHandler;
        private readonly UserState _userState;
        private readonly DialogSet _dialogSet;
        private readonly IStatePropertyAccessor<DialogState> _dialogState;
        private readonly BotHandlerChain _botHandlerChain;

        public Bot(ConversationState conversationState, EntryFillDialog fillDialog,
            LuisRecognizerProxy luisRecognizer,
            ReportDialog reportDialog, ClockifySetupDialog clockifySetupDialog, UserState userState, 
            StopReminderDialog stopReminderDialog, IClockifyService clockifyService, DicHandler dicHandler, BotHandlerChain botHandlerChain)
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
            _dicHandler = dicHandler;
            _botHandlerChain = botHandlerChain;
            _dialogSet = new DialogSet(_dialogState)
                .Add(_clockifySetupDialog)
                .Add(_fillDialog)
                .Add(_reportDialog)
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
            
            if (await RunClockifySetupIfNeeded(turnContext, cancellationToken, userProfile)) return;

            if(await _botHandlerChain.Handle(turnContext, cancellationToken, userProfile)) return;
            
            bool anyActiveDialog = dialogContext.ActiveDialog != null;
            if (!anyActiveDialog)
            {
                var (topIntent, entities) =
                    await _luisRecognizer.RecognizeAsyncIntent(turnContext, cancellationToken);

                bool intentHasBeenHandled = await IntentManager.HandleIntent(dialogContext, topIntent, cancellationToken, 
                    turnContext, entities, _dialogSet);
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