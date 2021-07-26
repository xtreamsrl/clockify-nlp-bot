using System.Security;

namespace Bot.Security
{
    public class MissingApiKeyException : SecurityException
    {
        public MissingApiKeyException(string message)
            : base(message)
        {
        }
    }
}