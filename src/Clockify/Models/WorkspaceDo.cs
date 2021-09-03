namespace Bot.Clockify.Models
{
    public class WorkspaceDo
    {
        public WorkspaceDo(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }
    }
}