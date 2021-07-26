using Castle.Core.Internal;

namespace Bot.Security
{
    public class ProactiveBotApiKeyValidator : IProactiveBotApiKeyValidator
    {
        private readonly IProactiveApiKeyProvider _proactiveApiKeyProvider;

        public ProactiveBotApiKeyValidator(IProactiveApiKeyProvider proactiveApiKeyProvider)
        {
            _proactiveApiKeyProvider = proactiveApiKeyProvider;
        }

        public void Validate(string clientApiKey)
        {
            string serverApiKey = _proactiveApiKeyProvider.GetApiKey();

            if (serverApiKey.IsNullOrEmpty())
            {
                return;
            }
            
            if (clientApiKey.IsNullOrEmpty() && !serverApiKey.IsNullOrEmpty())
            {
                throw new MissingApiKeyException("Proactive api key is missing");
            }
            
            if (clientApiKey != serverApiKey)
            {
                throw new InvalidApiKeyException("Api keys don't match");
            }
        }
    }
}