using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bot.States
{
    public interface IUserProfilesProvider
    {
        Task<List<UserProfile>> GetUserProfilesAsync();
    }
}