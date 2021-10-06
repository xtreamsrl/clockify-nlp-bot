using System;
using System.Threading.Tasks;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;

namespace Bot.Data
{
    internal class TokenRepository : ITokenRepository
    {
        private IMemoryCache _cache;
        private readonly SecretClient _secretClient;
        
        // TODO add caching

        public TokenRepository(IMemoryCache cache, SecretClient secretClient)
        {
            _cache = cache;
            _secretClient = secretClient;
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
            _cache.Set(tokenData.Id, tokenData);
            return tokenData;
        }
    }
}