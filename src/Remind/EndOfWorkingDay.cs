using System;
using System.Threading.Tasks;
using Bot.Common;
using Bot.States;

namespace Bot.Remind
{
    public class EndOfWorkingDay : INeedRemindService
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public EndOfWorkingDay(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public Task<bool> ReminderIsNeeded(UserProfile userProfile)
        {
            TimeZoneInfo userTimeZone = userProfile.TimeZone;
            var userTime = TimeZoneInfo.ConvertTime(_dateTimeProvider.DateTimeUtcNow(), userTimeZone);

            if (userTime.DayOfWeek == DayOfWeek.Saturday || userTime.DayOfWeek == DayOfWeek.Sunday)
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(userTime.Hour >= 17 && userTime.Hour <= 23);
        }
    }
}