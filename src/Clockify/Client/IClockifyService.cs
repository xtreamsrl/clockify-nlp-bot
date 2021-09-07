using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Clockify.Models;

namespace Bot.Clockify.Client
{
    public interface IClockifyService
    {
        public Task<UserDo> GetCurrentUserAsync(string apiKey);

        public Task<List<WorkspaceDo>> GetWorkspacesAsync(string apiKey);

        public Task<List<ClientDo>> GetClientsAsync(string apiKey, string workspaceId);

        public Task<List<ProjectDo>> GetProjectsAsync(string apiKey, string workspaceId);

        public Task<List<ProjectDo>> GetProjectsByClientsAsync(string apiKey,
            string workspaceId, IEnumerable<string> clients);

        public Task<List<TaskDo>> GetTasksAsync(string apiKey, string workspaceId,
            string projectId);

        public Task<List<HydratedTimeEntryDo>> GetHydratedTimeEntriesAsync(string apiKey,
            string workspaceId, string userId, DateTimeOffset? start = null, DateTimeOffset? end = null);

        public Task<TimeEntryDo> AddTimeEntryAsync(string apiKey, string workspaceId, TimeEntryReq timeEntryRequest);

        public Task DeleteTimeEntry(string apiKey, string workspaceId, string timeEntryId);

        public Task<string?> GetTagAsync(string apiKey, string workspaceId, string tagName);

        public Task<TaskDo> CreateTaskAsync(string apiKey, string taskName, string projectId, string workspaceId);
    }
}