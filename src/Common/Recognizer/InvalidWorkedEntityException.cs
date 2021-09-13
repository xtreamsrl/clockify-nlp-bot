using System;

namespace Bot.Common.Recognizer
{
    public class InvalidWorkedEntityException : Exception
    {
        public InvalidWorkedEntityException(string message) : base(message)
        {
        }
    }
}