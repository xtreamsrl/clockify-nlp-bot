using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace Bot.Clockify
{
    public interface IFollowUpService
    {
        Task<string> SendFollowUpAsync(BotAdapter adapter);
    }
}