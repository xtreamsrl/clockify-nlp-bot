using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Data;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Bot.Services.Reminds
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