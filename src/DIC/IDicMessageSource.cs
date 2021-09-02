using System;

namespace Bot.DIC
{
    public interface IDicMessageSource
    {
        string SetupRequest { get; }
        string SetupReject { get; }
        string SetupFeedback { get; }

        string NotificationSent { get; }
        string NotificationQuestion { get; }
        
        string NextWeekDayRetry { get; }
        string NextWeekDaySelection { get; }
        string NextWeekFeedback { get; }
        string NextWeekNeedForPermission { get; }
        string LongTermDayRetry { get; }
        string LongTermFeedback { get; }
        string LongTermNeedForPermission { get; }
        string LongTermRemoteSchedule { get; }
    }
}