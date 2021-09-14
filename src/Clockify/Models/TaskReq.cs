namespace Bot.Clockify.Models
{
    public class TaskReq
    {
        public TaskReq(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}