using System;

namespace Bot.Clockify.Fill
{
    public class InvalidWorkedEntityException : Exception
    {
        public InvalidWorkedEntityException(string message) : base(message)
        {
        }
    }
}