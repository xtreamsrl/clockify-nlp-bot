using Bot.Clockify.Models;
using Clockify.Net.Models.Projects;

namespace Bot.Clockify.Client
{
    internal static class ClockifyModelFactory
    {
        public static ProjectDo ToProjectDo(ProjectDtoImpl p)
        {
            return new ProjectDo
            {
                Id = p.Id,
                Name = p.Name,
                Archived = p.Archived,
                Billable = p.Billable,
                WorkspaceId = p.WorkspaceId
            };
        }
    }
}