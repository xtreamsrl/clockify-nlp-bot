using Microsoft.Extensions.Localization;

namespace Bot.Clockify
{
    public class ClockifyMessageSource : IClockifyMessageSource
    {
        private readonly IStringLocalizer<ClockifyMessageSource> _localizer;

        public ClockifyMessageSource(IStringLocalizer<ClockifyMessageSource> localizer)
        {
            _localizer = localizer;
        }

        public string SetupRequest => GetString(nameof(SetupRequest));
        public string SetupFeedback => GetString(nameof(SetupRequest));
        public string SetupReject => GetString(nameof(SetupReject));

        private string GetString(string name) => _localizer[name];
    }
}