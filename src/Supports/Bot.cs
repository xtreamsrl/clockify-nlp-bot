using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Bot.Supports
{
    public class Bot : ActivityHandler
    {
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private readonly BotHandlerChain _botHandlerChain;

        public Bot(ConversationState conversationState, UserState userState, BotHandlerChain botHandlerChain)
        {
            _conversationState = conversationState;
            _userState = userState;
            _botHandlerChain = botHandlerChain;
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
            var userProfile =
                await StaticUserProfileHelper.GetUserProfileAsync(_userState, turnContext, cancellationToken);
            userProfile.ConversationReference = turnContext.Activity.GetConversationReference();

            if (await _botHandlerChain.Handle(turnContext, cancellationToken, userProfile)) return;

            await turnContext.SendActivityAsync(MessageFactory.Text(
                    "mmm, I don't know exactly how to respond to " +
                    "that 😔... if you're stuck, just ask me for help"),
                cancellationToken);
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