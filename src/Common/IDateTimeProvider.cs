using System;

namespace Bot.Common
{
    public interface IDateTimeProvider
    {
        DateTime DateTimeNow();
    }
}