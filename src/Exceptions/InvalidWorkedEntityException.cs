using System;

namespace Bot.Exceptions
{
    public class InvalidWorkedEntityException : Exception
    {
        public InvalidWorkedEntityException(string message) : base(message)
        {
        }
    }
}