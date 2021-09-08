using System;
using System.Collections.Generic;

namespace Bot.Clockify.Models
{
    public class TimeEntryReq
    {
        public TimeEntryReq(string projectId, DateTimeOffset start, List<string>? tagIds = null, string? taskId = null,
            bool? billable = null, DateTimeOffset? end = null)
        {
            ProjectId = projectId;
            Start = start;
            TagIds = tagIds ?? new List<string>();
            TaskId = taskId;
            Billable = billable;
            End = end;
        }

        public string ProjectId { get; }

        public List<string>? TagIds { get; }
        
        public DateTimeOffset Start { get; }

        public string? TaskId { get; }

        public bool? Billable { get; }

        public DateTimeOffset? End { get; }
    }
}