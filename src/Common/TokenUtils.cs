using System;
using System.Threading.Tasks;
using Bot.Data;

namespace Bot.Common
{
    /// <summary>
    /// This class will be removed when all users token has been moved to Azure Key Vault
    /// </summary>
    public static class TokenUtils
    {
        public static async Task<string> GetToken(string? tokenId, string? tokenValue,
            ITokenRepository tokenRepository)
        {
            if (tokenValue == null) throw new ArgumentNullException(tokenValue);
            if (tokenId == null) throw new ArgumentNullException(tokenId);
            
            // TODO throw a domain exception or evaluate to refactor repository, in case of error we want to fail.
            TokenData token = await tokenRepository.ReadAsync(tokenId) ?? throw new Exception("Token not found");

            return token.Value;
        }
    }
}