using System;
using Microsoft.Bot.Schema;

namespace Bot.States
{
    public class UserProfile
    {
        public string? ClockifyTokenId { get; set; }
        
        public string? UserId { get; set; }
        
        public int? EmployeeId { get; set; }

        public string? DicTokenId { get; set; }

        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
        
        public ConversationReference? ConversationReference { get; set; }

        public DateTime? StopRemind { get; set; }
        public bool Experimental { get; set; }
        public string? Email { get; set; }
    }
}