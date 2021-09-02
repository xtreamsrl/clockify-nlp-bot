using Microsoft.Extensions.Localization;

namespace Bot.DIC
{
    public class DicMessageSource : IDicMessageSource
    {
        private readonly IStringLocalizer<DicMessageSource> _localizer;

        public DicMessageSource(IStringLocalizer<DicMessageSource> localizer)
        {
            _localizer = localizer;
        }

        public string SetupRequest => GetString(nameof(SetupRequest));
        public string SetupReject => GetString(nameof(SetupReject));
        public string SetupFeedback => GetString(nameof(SetupFeedback));
        
        public string NotificationSent => GetString(nameof(NotificationSent));
        public string NotificationQuestion => GetString(nameof(NotificationQuestion));

        public string NextWeekDayRetry => GetString(nameof(NextWeekDayRetry));
        public string NextWeekDaySelection => GetString(nameof(NextWeekDaySelection));
        public string NextWeekFeedback => GetString(nameof(NextWeekFeedback));
        public string NextWeekNeedForPermission => GetString(nameof(NextWeekNeedForPermission));

        public string LongTermDayRetry => GetString(nameof(LongTermDayRetry));
        public string LongTermFeedback => GetString(nameof(LongTermFeedback));
        public string LongTermNeedForPermission => GetString(nameof(LongTermNeedForPermission));
        public string LongTermRemoteSchedule => GetString(nameof(LongTermRemoteSchedule));

        private string GetString(string name) => _localizer[name];
    }
}