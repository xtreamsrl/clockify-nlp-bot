using System;

namespace Bot.Services.Reports
{
    public class DateTimeProvider: IDateTimeProvider
    {
        public DateTime DateTimeNow()
        {
            return DateTime.Now;
        }
    }
}