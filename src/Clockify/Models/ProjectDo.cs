namespace Bot.Clockify.Models
{
    public class ProjectDo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ClientId { get; set; }
        public bool? Billable { get; set; }
        public bool? Archived { get; set; }
        public string WorkspaceId { get; set; }
    }
}