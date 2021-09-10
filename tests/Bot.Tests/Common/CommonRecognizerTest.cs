using System;
using System.Collections.Generic;
using Bot.Common;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Bot.Tests.Common
{
    public class CommonRecognizerTest
    {
        [Fact]
        public void CommonRecognizer_LuisIsNotConfigured_ThrowsException()
        {
            var confOptions = new Dictionary<string, string>
            {
                { "Key1", "Value1" },
                { "Nested:Key1", "NestedValue1" },
                { "Nested:Key2", "NestedValue2" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(confOptions)
                .Build();

            Func<CommonRecognizer> action = () => new CommonRecognizer(configuration);

            action.Should().ThrowExactly<Exception>()
                .WithMessage(
                    "LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.");
        }

        [Fact]
        public void CommonRecognizer_LuisIsConfigured_CreatesCommonRecognizer()
        {
            var confOptions = new Dictionary<string, string>
            {
                { "LuisAppId", "3e92f182-af03-4bec-9c38-2e312abf4e8e" },
                { "LuisAPIKey", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" }, // must match /[0-9a-f]{32}/
                { "LuisAPIHostName", "westus.api.cognitive.microsoft.com" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(confOptions)
                .Build();
            
            var recognizer = new CommonRecognizer(configuration);
            recognizer.Should().NotBeNull();
        }
    }
}