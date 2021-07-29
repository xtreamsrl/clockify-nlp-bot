using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clockify.Net.Models.Clients;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Tags;
using Clockify.Net.Models.Tasks;
using Clockify.Net.Models.TimeEntries;
using Clockify.Net.Models.Users;
using Clockify.Net.Models.Workspaces;
using RestSharp;

namespace Bot.Services.Clockify
{
    public interface IClockifyClient
    {
        Task<IRestResponse<List<TaskDto>>> FindAllTasksAsync(
            string workspaceId,
            string projectId,
            bool? isActive = null,
            string? name = null,
            int page = 1,
            int pageSize = 50);

        Task<IRestResponse<TaskDto>> CreateTaskAsync(string workspaceId, string projectId, TaskRequest taskRequest);

        Task<IRestResponse<CurrentUserDto>> GetCurrentUserAsync();

        Task<IRestResponse<List<WorkspaceDto>>> GetWorkspacesAsync();

        Task<IRestResponse<List<ClientDto>>> FindAllClientsOnWorkspaceAsync(string workspaceId);

        Task<IRestResponse<List<ProjectDtoImpl>>> FindAllProjectsOnWorkspaceAsync(
            string workspaceId,
            int page = 1,
            int pageSize = 50);

        Task<IRestResponse<List<TagDto>>> FindAllTagsOnWorkspaceAsync(string workspaceId);

        Task<IRestResponse<TimeEntryDtoImpl>> CreateTimeEntryAsync(string workspaceId,
            TimeEntryRequest timeEntryRequest);

        Task<IRestResponse> DeleteTimeEntryAsync(string workspaceId, string timeEntryId);

        public Task<IRestResponse<List<ProjectDtoImpl>>> FindAllProjectsOnWorkspaceByClientsAsync(string workspaceId,
            IEnumerable<string> clients);

        public Task<IRestResponse<List<HydratedTimeEntryDtoImpl>>> FindAllHydratedTimeEntriesForUserAsync(
            string workspaceId,
            string userId,
            string? description = null,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string? project = null,
            string? task = null,
            bool? projectRequired = null,
            bool? taskRequired = null,
            bool? considerDurationFormat = null,
            bool? inProgress = null,
            int page = 1,
            int pageSize = 50);
    }
}