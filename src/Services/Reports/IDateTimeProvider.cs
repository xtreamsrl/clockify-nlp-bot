using System;

namespace Bot.Services.Reports
{
    public interface IDateTimeProvider
    {
        DateTime DateTimeNow();
    }
}