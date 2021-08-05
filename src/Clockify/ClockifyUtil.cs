using System;
using System.Threading.Tasks;
using Bot.Data;
using Bot.States;

namespace Bot.Clockify
{
    /// <summary>
    /// This class will be removed when all users token has been moved to Azure Key Vault
    /// </summary>
    public static class ClockifyUtil
    {
        public static async Task<string> GetClockifyToken(UserProfile userProfile, ITokenRepository tokenRepository)
        {
            if (userProfile.ClockifyToken == null)
            {
                throw new ArgumentNullException(userProfile.ClockifyToken);
            }
            
            // save securely clockify token
            if (userProfile.ClockifyTokenId == null)
            {
                var savedToken = await tokenRepository.WriteAsync(userProfile.ClockifyToken);
                userProfile.ClockifyTokenId = savedToken.Id;
                return userProfile.ClockifyToken;
            }
            
            // TODO throw a domain exception
            // New users do not have ClockifyToken populated
            TokenData token = await tokenRepository.ReadAsync(userProfile.ClockifyTokenId) ??
                              throw new Exception("Token not found");

            return token.Value;
        }
    }
}