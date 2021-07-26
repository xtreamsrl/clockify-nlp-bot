using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.States;

namespace Bot.Services.Reminds
{
    public interface IUserProfilesProvider
    {
        Task<List<UserProfile>> GetUserProfilesAsync();
    }
}