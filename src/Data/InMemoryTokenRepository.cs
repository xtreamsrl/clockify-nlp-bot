using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bot.Data
{
    public class InMemoryTokenRepository : ITokenRepository
    {
        private ConcurrentDictionary<string, string> _store = new ConcurrentDictionary<string, string>();


        private bool SaveStorageToFile()
        {
            var jsonStorage = JsonConvert.SerializeObject(_store);
            try
            {
                File.WriteAllText("jsonStorage.json",jsonStorage);
            }
            catch (Exception e)
            {
                throw new Exception("Error during writing of local storage with message: "+ e.Message);
            }

            return true;
        }
        
        public Task<TokenData> ReadAsync(string id)
        {
            if (id == null) throw new ArgumentNullException(id);

            //If local storage exists, load it!
            if (File.Exists("jsonStorage.json"))
            {
                var jsonStorage = File.ReadAllText("jsonStorage.json");
                _store = JsonConvert.DeserializeObject<ConcurrentDictionary<String, String>>(jsonStorage);
            }

            if (!_store.TryGetValue(id, out var value))
            {
                throw new TokenNotFoundException("No token has been found with id " + id);
            }
            return Task.FromResult(new TokenData(id, value));
        }

        public Task<bool> RemoveAsync(string id)
        {
            if (!_store.TryGetValue(id, out var _))
            {
                throw new TokenNotFoundException("No token has been found with id " + id);
            }

            //Removes the key from the store. 
            _store.Remove(id, out _);
            SaveStorageToFile();
            return Task.FromResult(true);
        }

        public Task<TokenData> WriteAsync(string value, string? id = null)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(value);

            string name = id ?? Guid.NewGuid().ToString();
            _store.AddOrUpdate(name, value, (key, current) => value);
            SaveStorageToFile();
            return Task.FromResult(new TokenData(name, value));
        }
    }
}
