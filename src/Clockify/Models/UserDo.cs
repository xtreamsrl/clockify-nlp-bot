namespace Bot.Clockify.Models
{
    public class UserDo
    {
        public string Id { get; set; }
        
        public string ActiveWorkspace { get; set; }

        public string DefaultWorkspace { get; set; }

        public string Email { get; set; }

        // IANA timezone
        public string TimeZone { get; set; }
        
        public string? Name { get; set; }
    }
}