using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;

namespace Bot.States
{
    public class UserProfilesProvider : IUserProfilesProvider
    {
        private readonly IStorage _storage;
        private readonly IAzureBlobReader _azureBlobReader;
        private readonly IUserProfileStorageReader _userProfileStorageReader;

        public UserProfilesProvider(
            IStorage storage, 
            IAzureBlobReader azureBlobReader, 
            IUserProfileStorageReader userProfileStorageReader)
        {
            _storage = storage;
            _azureBlobReader = azureBlobReader;
            _userProfileStorageReader = userProfileStorageReader;
        }

        public async Task<List<UserProfile>> GetUserProfilesAsync()
        {
            switch (_storage)
            {
                case MemoryStorage _:
                    // TODO How to read user profiles from MemoryStorage?
                    return new List<UserProfile>();
                case AzureBlobStorage _:
                {
                    var userKeys = _azureBlobReader.GetUserKeys();
                    return await _userProfileStorageReader.GetUsersData(userKeys);
                }
                default:
                    throw new NotSupportedException("Only memory storage and blob storage are supported");
            }
        }
    }
}