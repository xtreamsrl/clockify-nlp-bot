using System;
using System.Collections.Generic;

namespace Bot.Clockify.Models
{
    public class HydratedTimeEntryDo
    {
        public HydratedTimeEntryDo(string id, ProjectDo project, TimeInterval timeInterval, List<TagDo> tags, 
            TaskDo? task = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Tags = tags ?? throw new ArgumentNullException(nameof(tags));
            Task = task;
            TimeInterval = timeInterval ?? throw new ArgumentNullException(nameof(timeInterval));
        }
        
        public HydratedTimeEntryDo(string id, ProjectDo project, TimeInterval timeInterval, TaskDo? task = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Tags = new List<TagDo>();
            Task = task;
            TimeInterval = timeInterval ?? throw new ArgumentNullException(nameof(timeInterval));
        }

        public string Id { get; }
        public ProjectDo Project { get; }
        public List<TagDo> Tags { get; }
        public TaskDo? Task { get; }
        public TimeInterval TimeInterval { get; }
    }
    
}