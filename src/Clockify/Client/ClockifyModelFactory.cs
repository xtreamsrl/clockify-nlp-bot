using System.Linq;
using Bot.Clockify.Models;
using Clockify.Net.Models.Clients;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Tags;
using Clockify.Net.Models.Tasks;
using Clockify.Net.Models.TimeEntries;
using Clockify.Net.Models.Users;
using Clockify.Net.Models.Workspaces;

namespace Bot.Clockify.Client
{
    internal static class ClockifyModelFactory
    {
        public static WorkspaceDo ToWorkspaceDo(WorkspaceDto w)
        {
            return new WorkspaceDo(w.Id, w.Name);
        }

        public static ProjectDo ToProjectDo(ProjectDtoImpl p)
        {
            return new ProjectDo
            {
                Id = p.Id,
                Name = p.Name,
                ClientId = p.ClientId,
                Archived = p.Archived,
                Billable = p.Billable,
                WorkspaceId = p.WorkspaceId
            };
        }

        public static TaskDo ToTaskDo(TaskDto t)
        {
            var statusDo = t.Status == TaskStatus.Active ? TaskStatusDo.Active : TaskStatusDo.Done;

            return new TaskDo
            {
                Id = t.Id,
                Name = t.Name,
                ProjectId = t.ProjectId,
                Status = statusDo
            };
        }

        public static HydratedTimeEntryDo ToHydratedTimeEntryDo(HydratedTimeEntryDtoImpl entry)
        {
            return new HydratedTimeEntryDo
            {
                Id = entry.Id,
                Project = ToProjectDo(entry.Project),
                Task = entry.Task != null ? ToTaskDo(entry.Task) : null,
                Tags = entry.Tags.Select(ToTagDo).ToList(),
                TimeInterval = ToTimeInterval(entry.TimeInterval)
            };
        }

        public static TagDo ToTagDo(TagDto t)
        {
            return new TagDo(t.Id, t.Name);
        }

        public static TimeInterval ToTimeInterval(TimeIntervalDto interval)
        {
            return new TimeInterval
            {
                Start = interval.Start,
                End = interval.End
            };
        }

        public static TimeEntryDo ToTimeEntryDo(TimeEntryDtoImpl entry)
        {
            return new TimeEntryDo
            {
                Id = entry.Id,
                ProjectId = entry.ProjectId,
                TaskId = entry.TaskId,
                UserId = entry.UserId,
                Billable = entry.Billable,
                TagIds = entry.TagIds,
                TimeInterval = ToTimeInterval(entry.TimeInterval)
            };
        }

        public static TimeEntryRequest ToTimeEntryRequest(TimeEntryReq entry)
        {
            return new TimeEntryRequest
            {
                ProjectId = entry.ProjectId,
                TaskId = entry.TaskId,
                UserId = entry.UserId,
                Billable = entry.Billable,
                TagIds = entry.TagIds,
                Start = entry.TimeInterval.Start,
                End = entry.TimeInterval.End
            };
        }

        public static UserDo ToUserDo(CurrentUserDto u)
        {
            return new UserDo
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                ActiveWorkspace = u.ActiveWorkspace,
                DefaultWorkspace = u.DefaultWorkspace
            };
        }

        public static ClientDo ToClientDo(ClientDto c)
        {
            return new ClientDo(c.Id, c.Name, c.WorkspaceId);
        }
    }
}