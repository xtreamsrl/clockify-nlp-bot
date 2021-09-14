namespace Bot.Clockify.Models
{
    public class TagDo
    {
        public TagDo(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }
    }
}