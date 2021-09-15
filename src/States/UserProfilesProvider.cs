using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bot.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Newtonsoft.Json.Linq;

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
                case MemoryStorage m:
                    try
                    {
                        const BindingFlags fs = BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance;
                        var memory = (Dictionary<string, JObject>) 
                            typeof(MemoryStorage).GetField("_memory", fs)!.GetValue(m)!;
                        var users = memory
                            .Where(k => k.Key.Contains("user"))
                            .Select(x => x.Value)
                            .Where(j => j.ContainsKey("UserProfile"))
                            .Select(j => j.GetValue("UserProfile")!.ToObject<UserProfile>()!);
                        return users.ToList();
                    }
                    catch (Exception)
                    {
                        return new List<UserProfile>();
                    }
                case AzureBlobStorage _:
                {
                    string[] userKeys = _azureBlobReader.GetUserKeys();
                    return await _userProfileStorageReader.GetUsersData(userKeys);
                }
                default:
                    throw new NotSupportedException("Only memory storage and blob storage are supported");
            }
        }
    }
}