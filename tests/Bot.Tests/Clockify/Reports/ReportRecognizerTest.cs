using System.Collections.Generic;
using Bot.Clockify;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Bot.Tests.Clockify.Reports
{
    public class ReportRecognizerTest
    {
        [Fact]
        public void WhenNotConfigured_IsConfiguredShouldReturnFalse()
        {
            var confOptions = new Dictionary<string, string>
            {
                {"Key1", "Value1"},
                {"Nested:Key1", "NestedValue1"},
                {"Nested:Key2", "NestedValue2"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(confOptions)
                .Build();
            var luisRecognizer = new LuisRecognizerProxy(configuration);
            luisRecognizer.IsConfigured.Should().BeFalse();
        }

        [Fact]
        public void WhenConfigured_IsConfiguredShouldReturnTrue()
        {
            var confOptions = new Dictionary<string, string>
            {
                {"LuisAppId", "3e92f182-af03-4bec-9c38-2e312abf4e8e"},
                {"LuisAPIKey", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"}, // must match /[0-9a-f]{32}/
                {"LuisAPIHostName", "westus.api.cognitive.microsoft.com"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(confOptions)
                .Build();
            var luisRecognizer = new LuisRecognizerProxy(configuration);
            luisRecognizer.IsConfigured.Should().BeTrue();
        }
    }
}