using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bot.Data
{
    public class InMemoryTokenRepository : ITokenRepository
    {
        private ConcurrentDictionary<string, string> _store = new ConcurrentDictionary<string, string>();
        

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

        public Task<TokenData> WriteAsync(string value, string? id = null)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(value);

            string name = id ?? Guid.NewGuid().ToString();
            _store.AddOrUpdate(name, value, (key, current) => value);
            var jsonStorage = JsonConvert.SerializeObject(_store);
            File.WriteAllText("jsonStorage.json",jsonStorage);
            return Task.FromResult(new TokenData(name, value));
        }
    }
}
