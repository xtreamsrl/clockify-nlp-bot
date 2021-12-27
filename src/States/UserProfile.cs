using System;
using Microsoft.Bot.Schema;
using TimeZoneConverter;

namespace Bot.States
{
    public class UserProfile
    {
        private TimeZoneInfo? _timeZone;
        public string? ClockifyTokenId { get; set; }
        
        public string? UserId { get; set; }
        
        public int? EmployeeId { get; set; }

        public string? DicTokenId { get; set; }

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
        
        public ConversationReference? ConversationReference { get; set; }
        
        public double? WorkingHours { get; set; }

        public DateTime? StopRemind { get; set; }
        public bool Experimental { get; set; }
        public string? Email { get; set; }
        public TimeZoneInfo TimeZone {
            get => _timeZone ?? TZConvert.GetTimeZoneInfo("Europe/Rome");
            set => _timeZone = value;
        }

        public DateTime? LastFollowUpTimestamp { get; set; }
        
        public DateTime? LastConversationUpdate { get; set; }
    }
}