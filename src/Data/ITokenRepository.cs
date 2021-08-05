using System.Threading.Tasks;

namespace Bot.Data
{
    public interface ITokenRepository
    {
        /// <summary>
        /// Read the token data starting from a string identifier
        /// </summary>
        /// <param name="name">The token identifier</param>
        /// <returns>The token value</returns>
        Task<TokenData?> ReadAsync(string name);

      
        // TODO doc
        Task<TokenData> WriteAsync(string name, string value);
    }
}