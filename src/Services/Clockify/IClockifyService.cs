using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clockify.Net.Models.Clients;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Tasks;
using Clockify.Net.Models.TimeEntries;
using Clockify.Net.Models.Users;
using Clockify.Net.Models.Workspaces;

namespace Bot.Services.Clockify
{
    public interface IClockifyService
    {
        public Task<CurrentUserDto> GetCurrentUserAsync(string apiKey);

        public Task<List<WorkspaceDto>> GetWorkspacesAsync(string apiKey);

        public Task<List<ClientDto>> GetClientsAsync(string apiKey, string workspaceId);

        public Task<List<ProjectDtoImpl>> GetProjectsAsync(string apiKey, string workspaceId);

        public Task<List<ProjectDtoImpl>> GetProjectsByClientsAsync(string apiKey,
            string workspaceId, IEnumerable<string> clients);

        public Task<List<TaskDto>> GetTasksAsync(string apiKey, string workspaceId,
            string projectId);

        public Task<List<HydratedTimeEntryDtoImpl>> GetHydratedTimeEntriesAsync(string apiKey,
            string workspaceId, string userId, DateTimeOffset? start = null, DateTimeOffset? end = null);

        public Task<TimeEntryDtoImpl> AddTimeEntryAsync(string apiKey, string workspaceId,
            TimeEntryRequest timeEntryRequest);

        public Task DeleteTimeEntry(string apiKey, string workspaceId, string timeEntryId);

        public Task<string?> GetTagAsync(string apiKey, string workspaceId, string tagName);

        public Task<TaskDto> CreateTaskAsync(string apiKey, string taskName, string projectId, string workspaceId);
    }
}