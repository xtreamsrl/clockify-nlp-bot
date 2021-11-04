using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.States;
using Microsoft.Bot.Builder;

namespace Bot.Clockify
{
    public interface IFollowUpService
    {
        Task<List<UserProfile>> SendFollowUpAsync(BotAdapter adapter);
    }
}