using Bot.Clockify;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bot.Tests.Clockify
{
    public class TestClockifyUtils
    {
        internal static IClockifyMessageSource ClockifyMessageSource()
        {
            var options = Options.Create(new LocalizationOptions {ResourcesPath = "Common/Resources"});
            var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
            var localizer = new StringLocalizer<ClockifyMessageSource>(factory);
            return new ClockifyMessageSource(localizer);
        }
    }
}