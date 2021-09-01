using System.Threading;
using System.Threading.Tasks;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Supports
{
    public interface IBotHandler
    {
        Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken, UserProfile userProfile);

        DialogSet GetDialogSet();
    }
}