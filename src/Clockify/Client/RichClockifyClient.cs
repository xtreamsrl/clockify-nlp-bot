using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clockify.Net;
using Clockify.Net.Models.Clients;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Tags;
using Clockify.Net.Models.Tasks;
using Clockify.Net.Models.TimeEntries;
using Clockify.Net.Models.Users;
using Clockify.Net.Models.Workspaces;
using RestSharp;

namespace Bot.Clockify.Client
{
    public class RichClockifyClient : IClockifyClient
    {
        private readonly ClockifyClient _clockifyClient;

        public RichClockifyClient(string apiKey)
        {
            _clockifyClient = new ClockifyClient(apiKey);
        }

        public Task<IRestResponse> DeleteTimeEntryAsync(string workspaceId, string timeEntryId)
        {
            return _clockifyClient.DeleteTimeEntryAsync(workspaceId, timeEntryId);
        }

        public Task<IRestResponse<List<ProjectDtoImpl>>> FindAllProjectsOnWorkspaceByClientsAsync(
            string workspaceId, IEnumerable<string> clients)
        {
            return _clockifyClient.FindAllProjectsOnWorkspaceAsync(workspaceId, clients: clients.ToArray());
        }

        public Task<IRestResponse<List<HydratedTimeEntryDtoImpl>>> FindAllHydratedTimeEntriesForUserAsync(
            string workspaceId, string userId, string? description = null,
            DateTimeOffset? start = null, DateTimeOffset? end = null, string? project = null, string? task = null,
            bool? projectRequired = null, bool? taskRequired = null, bool? considerDurationFormat = null,
            bool? inProgress = null, int page = 1, int pageSize = 50)
        {
            return _clockifyClient.FindAllHydratedTimeEntriesForUserAsync(workspaceId, userId, description, start, end,
                project, task, projectRequired, taskRequired, considerDurationFormat, inProgress, page, pageSize);
        }

        public Task<IRestResponse<List<TaskDto>>> FindAllTasksAsync(string workspaceId, string projectId,
            bool? isActive = null, string? name = null, int page = 1,
            int pageSize = 50)
        {
            return _clockifyClient.FindAllTasksAsync(workspaceId, projectId, isActive, name, page, pageSize);
        }

        public Task<IRestResponse<TaskDto>> CreateTaskAsync(string workspaceId, string projectId,
            TaskRequest taskRequest)
        {
            return _clockifyClient.CreateTaskAsync(workspaceId, projectId, taskRequest);
        }

        public Task<IRestResponse<CurrentUserDto>> GetCurrentUserAsync()
        {
            return _clockifyClient.GetCurrentUserAsync();
        }

        public Task<IRestResponse<List<WorkspaceDto>>> GetWorkspacesAsync()
        {
            return _clockifyClient.GetWorkspacesAsync();
        }

        public Task<IRestResponse<List<ClientDto>>> FindAllClientsOnWorkspaceAsync(string workspaceId)
        {
            return _clockifyClient.FindAllClientsOnWorkspaceAsync(workspaceId);
        }

        public Task<IRestResponse<List<ProjectDtoImpl>>> FindAllProjectsOnWorkspaceAsync(
            string workspaceId,
            int page = 1,
            int pageSize = 50)
        {
            return _clockifyClient.FindAllProjectsOnWorkspaceAsync(workspaceId, page: page, pageSize: pageSize);
        }

        public Task<IRestResponse<List<TagDto>>> FindAllTagsOnWorkspaceAsync(string workspaceId)
        {
            return _clockifyClient.FindAllTagsOnWorkspaceAsync(workspaceId);
        }

        public Task<IRestResponse<TimeEntryDtoImpl>> CreateTimeEntryAsync(string workspaceId,
            TimeEntryRequest timeEntryRequest)
        {
            return _clockifyClient.CreateTimeEntryAsync(workspaceId, timeEntryRequest);
        }
        
    }
}