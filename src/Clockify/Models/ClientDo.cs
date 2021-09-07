namespace Bot.Clockify.Models
{
    public class ClientDo
    {
        public string Id { get; }

        public string Name { get; }

        public string WorkspaceId { get; }

        public ClientDo(string id, string name, string workspaceId)
        {
            Id = id;
            Name = name;
            WorkspaceId = workspaceId;
        }
    }
}