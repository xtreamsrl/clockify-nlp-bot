using System;
using System.Collections.Generic;

namespace Bot.Clockify.Models
{
    public class HydratedTimeEntryDo
    {
        public string Id { get; set; }
        public ProjectDo Project { get; set; }
        public List<TagDo> Tags { get; set; }
        public TaskDo? Task { get; set; }
        public TimeInterval TimeInterval { get; set; }
    }

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