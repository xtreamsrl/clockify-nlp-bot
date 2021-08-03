using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.States;
using Microsoft.Bot.Builder;

namespace Bot.Supports
{
    public class BotHandlerChain
    {
        private readonly IEnumerable<IBotHandler> _botHandlers;

        public BotHandlerChain(IEnumerable<IBotHandler> botHandlers)
        {
            _botHandlers = botHandlers;
        }

        // TODO Evaluate to implement chain of responsibility
        public async Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken,
            UserProfile userProfile)
        {
            foreach (var botHandler in _botHandlers)
            {
                bool result = await botHandler.Handle(turnContext, cancellationToken, userProfile);
                if (result) return result;
            }

            return false;
        }
    }
}