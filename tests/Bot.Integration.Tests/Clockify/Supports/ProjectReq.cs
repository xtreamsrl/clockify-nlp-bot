#nullable enable
namespace Bot.Integration.Tests.Clockify.Supports
{
    public class ProjectReq
    {
        public ProjectReq(string name)
        {
            Name = name;
        }

        public string Name { get; }
        
        public string? ClientId { get; set; }
        public bool? IsPublic { get; set; }
        public bool? Billable { get; set; }
    }
}