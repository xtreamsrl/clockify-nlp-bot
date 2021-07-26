using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.States;

namespace Bot.Data
{
    public interface IUserProfileStorageReader
    {
        Task<List<UserProfile>> GetUsersData(string[] userKeys);
    }
}