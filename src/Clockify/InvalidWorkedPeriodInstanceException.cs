using System;

namespace Bot.Clockify
{
    // TODO Rearrange exceptions so that Fill and reports do not share this one
    public class InvalidWorkedPeriodInstanceException : Exception
    {
        public InvalidWorkedPeriodInstanceException(string message) : base(message)
        {
        }
    }
}