using System.Threading.Tasks;

namespace Bot.Data
{
    public interface ITokenRepository
    {
        /// <summary>
        /// Read the token data starting from a string identifier
        /// </summary>
        /// <param name="id">The token identifier</param>
        /// <returns>The token value</returns>
        Task<TokenData?> ReadAsync(string id);

      
        // TODO doc
        Task<TokenData> WriteAsync(string value);
    }
}