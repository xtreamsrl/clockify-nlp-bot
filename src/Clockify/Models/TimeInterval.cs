using System;

namespace Bot.Clockify.Models
{
    public class TimeInterval
    {
        public DateTimeOffset? End { get; set; }
        public DateTimeOffset? Start { get; set; }
    }
}