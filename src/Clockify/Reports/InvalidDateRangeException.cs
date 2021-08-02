using System;

namespace Bot.Clockify.Reports
{
    public class InvalidDateRangeException: Exception
    {
        public InvalidDateRangeException(string message) : base(message)
        {
        }
    }
}