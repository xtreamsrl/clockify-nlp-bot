using System.Threading;
using System.Threading.Tasks;
using Bot.Common;
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
        private readonly ICommonMessageSource _messageSource;
        private readonly IDateTimeProvider _dateTimeProvider;

        public Bot(ConversationState conversationState, UserState userState, BotHandlerChain botHandlerChain,
            ICommonMessageSource messageSource, IDateTimeProvider dateTimeProvider)
        {
            _conversationState = conversationState;
            _userState = userState;
            _botHandlerChain = botHandlerChain;
            _messageSource = messageSource;
            _dateTimeProvider = dateTimeProvider;
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
            userProfile.LastConversationUpdate = _dateTimeProvider.DateTimeUtcNow();
            
            if (await _botHandlerChain.Handle(turnContext, cancellationToken, userProfile)) return;
            
            await turnContext.SendActivityAsync(MessageFactory.Text(_messageSource.MessageUnhandled), cancellationToken);
        }
    }
}