using System;

namespace Bot.Common.Recognizer
{
    public class InvalidWorkedDurationException : Exception
    {
        public InvalidWorkedDurationException(string message) : base(message)
        {
        }
    }
}