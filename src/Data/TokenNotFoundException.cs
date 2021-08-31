using System;

namespace Bot.Data
{
    public class TokenNotFoundException : Exception
    {
        public TokenNotFoundException(string message) : base(message)
        {
        }
    }
}