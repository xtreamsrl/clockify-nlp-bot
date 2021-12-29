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
        /// <exception cref="TokenNotFoundException">The token could not be found.</exception>
        Task<TokenData> ReadAsync(string id);
        
        
        /// <summary>
        /// Removes the token data starting from a string identifier
        /// </summary>
        /// <param name="id">The token identifier</param>
        /// <returns>a boolean success indicator</returns>
        /// <exception cref="TokenNotFoundException">The token could not be found.</exception>
        Task<bool> RemoveAsync(string id);

      
        /// <summary>
        /// Write provided key value pair. If the id is null a new id will be automatically generated.
        /// </summary>
        /// <param name="value">The token value</param>
        /// <param name="id">The token identifier</param>
        /// <returns>Returns the saved token data</returns>
        Task<TokenData> WriteAsync(string value, string? id = null);
    }
}