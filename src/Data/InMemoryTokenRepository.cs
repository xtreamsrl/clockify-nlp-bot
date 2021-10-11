using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Bot.Data
{
    public class InMemoryTokenRepository : ITokenRepository
    {
        private readonly ConcurrentDictionary<string, string> _store = new ConcurrentDictionary<string, string>();

        public Task<TokenData> ReadAsync(string id)
        {
            if (id == null) throw new ArgumentNullException(id);

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
            return Task.FromResult(new TokenData(name, value));
        }
    }
}
