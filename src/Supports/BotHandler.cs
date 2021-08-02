using System.Threading;
using System.Threading.Tasks;
using Bot.States;
using Microsoft.Bot.Builder;

namespace Bot.Supports
{
    public interface IBotHandler
    {
        Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken, UserProfile userProfile);
    }
}