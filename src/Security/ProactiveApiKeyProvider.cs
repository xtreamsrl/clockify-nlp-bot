using Microsoft.Extensions.Configuration;
using static Bot.Security.ProactiveApiKeyUtil;

namespace Bot.Security
{
    public class ProactiveApiKeyProvider : IProactiveApiKeyProvider
    {
        private readonly string _proactiveApiToken;

        public ProactiveApiKeyProvider(IConfiguration configuration)
        {
            _proactiveApiToken = configuration[ProactiveBotApiKey] ?? "";
        }

        public string GetApiKey()
        {
            return _proactiveApiToken;
        }
    }
}