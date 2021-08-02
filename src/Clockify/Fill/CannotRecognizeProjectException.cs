using System;

namespace Bot.Clockify.Fill
{
    public class CannotRecognizeProjectException : Exception
    {
        public readonly string Unmatchable;

        public CannotRecognizeProjectException(string unmatchable) : base(unmatchable)
        {
            Unmatchable = unmatchable;
        }
    }
}