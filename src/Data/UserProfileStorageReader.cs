using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Services.Reminds;
using Bot.States;
using Microsoft.Bot.Builder;

namespace Bot.Data
{
    public class UserProfileStorageReader : IUserProfileStorageReader
    {
        private readonly IStorage _storage;

        public UserProfileStorageReader(IStorage storage)
        {
            _storage = storage;
        }

        public async Task<List<UserProfile>> GetUsersData(string[] userKeys)
        {
            var usersRawData = await _storage.ReadAsync(userKeys,
                CancellationToken.None);

            return usersRawData
                .Select(e => e.Value)
                .Select(e => e as IDictionary<string, object>)
                .Select(e => e?["UserProfile"] as UserProfile)
                .Where(u => u != null)
                .Select(u => u!)
                .ToList();
        }
    }
}