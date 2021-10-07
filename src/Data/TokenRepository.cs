using System;
using System.Threading.Tasks;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;

namespace Bot.Data
{
    public class TokenRepository : ITokenRepository
    {
        private readonly SecretClient _secretClient;
        private readonly IMemoryCache _cache;
        private readonly int _cacheSeconds;

        public TokenRepository(SecretClient secretClient, IMemoryCache cache, int cacheSeconds)
        {
            _cache = cache;
            _secretClient = secretClient;
            _cacheSeconds = cacheSeconds;
        }

        public async Task<TokenData> ReadAsync(string id)
        {
            if (id == null) throw new ArgumentNullException(id);

            if (_cache.TryGetValue<TokenData>(id, out var cachedTokenData))
            {
                return cachedTokenData;
            }

            try
            {
                KeyVaultSecret secret = await _secretClient.GetSecretAsync(id);
                return GetTokenData(secret);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == 404)
                {
                    throw new TokenNotFoundException("No token has been found with id " + id);
                }

                throw;
            }
        }

        public async Task<TokenData> WriteAsync(string value, string? id = null)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(value);
            
            string name = id ?? Guid.NewGuid().ToString();
            KeyVaultSecret secret = await _secretClient.SetSecretAsync(name, value);
            return GetTokenData(secret);
        }

        private TokenData GetTokenData(KeyVaultSecret secret)
        {
            var tokenData = new TokenData(secret.Name, secret.Value);
            _cache.Set(tokenData.Id, tokenData, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(_cacheSeconds) });
            return tokenData;
        }
    }
}