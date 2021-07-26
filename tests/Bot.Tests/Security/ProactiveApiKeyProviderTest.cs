using System.Collections.Generic;
using Bot.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;
using static Bot.Utils.ProactiveApiKeyUtil;

namespace Bot.Tests.Security
{
    public class ProactiveApiKeyProviderTest
    {
        [Fact]
        public void GetToken_ProactiveBotApiKeyIsMissingFromConfig_ReturnEmpty()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();
            var proactiveApiKeyProvider = new ProactiveApiKeyProvider(config);

            string apiKey = proactiveApiKeyProvider.GetApiKey();

            apiKey.Should().BeEmpty();
        }
        
        [Fact]
        public void GetToken_ProactiveBotApiKeyIsPresentInConfig_ReturnApiKeyInConfig()
        {
            const string apiKeyInConfig = "asdkjsahdjhkadshjkdas";
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {ProactiveBotApiKey, apiKeyInConfig}
                })
                .Build();
            var proactiveApiKeyProvider = new ProactiveApiKeyProvider(config);

            string apiKey = proactiveApiKeyProvider.GetApiKey();

            apiKey.Should().BeEquivalentTo(apiKeyInConfig);
        }
    }
}