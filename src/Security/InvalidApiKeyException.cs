using System.Security;

namespace Bot.Security
{
    public class InvalidApiKeyException : SecurityException
    {
        public InvalidApiKeyException(string message)
            : base(message)
        {
        }
    }
}