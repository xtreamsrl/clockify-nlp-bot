namespace Bot.Clockify.Models
{
    public class TaskDo
    {
        public string? Id { get; set; }

        public string Name { get; set; }

        public string ProjectId { get; set; }
        
        public TaskStatusDo Status { get; set; }
    }
    
    public enum TaskStatusDo
    {
        Active,
        Done
    }
    
}