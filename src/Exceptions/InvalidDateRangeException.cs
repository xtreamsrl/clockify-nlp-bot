using System;

namespace Bot.Exceptions
{
    public class InvalidDateRangeException: Exception
    {
        public InvalidDateRangeException(string message) : base(message)
        {
        }
    }
}