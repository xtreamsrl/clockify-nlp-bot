using System.Collections.Generic;
using System.Linq;
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
            if (await ContinueOngoingDialogIfAny(turnContext, cancellationToken)) return true;
            foreach (var botHandler in _botHandlers)
            {
                bool result = await botHandler.Handle(turnContext, cancellationToken, userProfile);
                if (result) return result;
            }
            return false;
        }

        private async Task<bool> ContinueOngoingDialogIfAny(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var ds in _botHandlers.Select(bh => bh.GetDialogSet()))
            {
                var dialogContext = await ds.CreateContextAsync(turnContext, cancellationToken);
                bool anyActiveDialog = dialogContext.ActiveDialog != null;
                if (!anyActiveDialog) continue;
                await dialogContext.ContinueDialogAsync(cancellationToken);
                return true;
            }

            return false;
        }
    }
}