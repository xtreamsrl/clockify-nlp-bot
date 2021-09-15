using System;

namespace Bot.Common
{
    public class DateTimeProvider: IDateTimeProvider
    {
        public DateTime DateTimeNow()
        {
            return DateTime.Now;
        }

        public DateTime DateTimeUtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}