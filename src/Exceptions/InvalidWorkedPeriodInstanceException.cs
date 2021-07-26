using System;

namespace Bot.Exceptions
{
    public class InvalidWorkedPeriodInstanceException : Exception
    {
        public InvalidWorkedPeriodInstanceException(string message) : base(message)
        {
        }
    }
}