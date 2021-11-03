using System;

namespace Bot.Common.Recognizer
{
    public class InvalidWorkedPeriodException : Exception
    {
        public InvalidWorkedPeriodException(string message) : base(message)
        {
        }
    }
}