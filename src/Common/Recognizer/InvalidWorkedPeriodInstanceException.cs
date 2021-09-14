using System;

namespace Bot.Common.Recognizer
{
    public class InvalidWorkedPeriodInstanceException : Exception
    {
        public InvalidWorkedPeriodInstanceException(string message) : base(message)
        {
        }
    }
}