using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Clockify.Models;
using Clockify.Net.Models.Clients;
using Clockify.Net.Models.Tasks;
using Clockify.Net.Models.Users;
using Microsoft.Bot.Schema;

namespace Bot.Clockify.Client
{
    // Read operations
    public partial class ClockifyService : IClockifyService
    {
        private const int PageSize = 200;
        private readonly IClockifyClientFactory _clockifyClientFactory;

        public ClockifyService(IClockifyClientFactory clockifyClientFactory)
        {
            _clockifyClientFactory = clockifyClientFactory;
        }

        public async Task<CurrentUserDto> GetCurrentUserAsync(string apiKey)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.GetCurrentUserAsync();

            if (!response.IsSuccessful) throw new ErrorResponseException("Unable to get current user");

            return response.Data;
        }

        public async Task<List<WorkspaceDo>> GetWorkspacesAsync(string apiKey)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.GetWorkspacesAsync();

            if (!response.IsSuccessful) throw new ErrorResponseException("Unable to get workspaces");

            return response.Data.Select(ClockifyModelFactory.ToWorkspaceDo).ToList();
        }

        public async Task<List<ClientDto>> GetClientsAsync(string apiKey, string workspaceId)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.FindAllClientsOnWorkspaceAsync(workspaceId);

            if (!response.IsSuccessful)
                throw new ErrorResponseException($"Unable to get clients for workspaceId {workspaceId}");

            return response.Data;
        }

        public async Task<List<ProjectDo>> GetProjectsAsync(string apiKey,
            string workspaceId)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.FindAllProjectsOnWorkspaceAsync(workspaceId, 1, PageSize);

            if (!response.IsSuccessful)
                throw new ErrorResponseException($"Unable to get projects for workspaceId {workspaceId}");

            return response.Data.Select(ClockifyModelFactory.ToProjectDo).ToList();
        }

        public async Task<List<ProjectDo>> GetProjectsByClientsAsync(string apiKey,
            string workspaceId, IEnumerable<string> clients)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.FindAllProjectsOnWorkspaceByClientsAsync(workspaceId, clients);

            if (!response.IsSuccessful)
                throw new ErrorResponseException(
                    $"Unable to get projects for workspaceId {workspaceId} and clients {clients}"
                );

            return response.Data.Select(ClockifyModelFactory.ToProjectDo).ToList();
        }

        public async Task<List<TaskDo>> GetTasksAsync(string apiKey, string workspaceId,
            string projectId)
        {
            // TODO Implement pagination? Clockify api do not put any total page in response body
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.FindAllTasksAsync(workspaceId, projectId, pageSize: PageSize);

            if (!response.IsSuccessful)
                throw new ErrorResponseException(
                    $"Unable to get tasks for workspaceId {workspaceId} and projectId {projectId}"
                );

            return response.Data.Select(ClockifyModelFactory.ToTaskDo).ToList();
        }

        public async Task<List<HydratedTimeEntryDo>> GetHydratedTimeEntriesAsync(
            string apiKey,
            string workspaceId,
            string userId,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null)
        {
            // TODO Implement pagination? Clockify api do not put any total page in response body
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.FindAllHydratedTimeEntriesForUserAsync(
                workspaceId,
                userId,
                start: start,
                end: end,
                pageSize: PageSize
            );

            if (!response.IsSuccessful)
                throw new ErrorResponseException(
                    $"Unable to get time entries for workspaceId {workspaceId} for user {userId}");

            return response.Data.Select(ClockifyModelFactory.ToHydratedTimeEntryDo).ToList();
        }


        public async Task<string?> GetTagAsync(string apiKey, string workspaceId, string tagName)
        {
            if (tagName == null)
            {
                return null;
            }

            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.FindAllTagsOnWorkspaceAsync(workspaceId);

            if (!response.IsSuccessful)
            {
                throw new ErrorResponseException("Unable to get tag");
            }

            return response.Data.Find(e => e.Name == tagName)?.Id;
        }

        public async Task<TaskDo> CreateTaskAsync(string apiKey, string taskName, string projectId, string workspaceId)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.CreateTaskAsync(workspaceId, projectId, new TaskRequest()
            {
                Name = taskName
            });
            if (!response.IsSuccessful)
                throw new ErrorResponseException(
                    $"Unable to create task for workspaceId {workspaceId} and projectId {projectId}"
                );
            return ClockifyModelFactory.ToTaskDo(response.Data);
        }
    }

    // Write operations
    public partial class ClockifyService
    {
        public async Task<TimeEntryDo> AddTimeEntryAsync(string apiKey, string workspaceId,
            TimeEntryReq timeEntryRequest)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.CreateTimeEntryAsync(workspaceId,
                ClockifyModelFactory.ToTimeEntryRequest(timeEntryRequest));

            if (!response.IsSuccessful)
                throw new ErrorResponseException(
                    $"Unable to add a new time entry - {timeEntryRequest} - for workspaceId {workspaceId}"
                );

            return ClockifyModelFactory.ToTimeEntryDo(response.Data);
        }

        public async Task DeleteTimeEntry(string apiKey, string workspaceId, string timeEntryId)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.DeleteTimeEntryAsync(workspaceId, timeEntryId);

            if (!response.IsSuccessful)
                throw new ErrorResponseException(
                    $"Unable to delete time entry - {timeEntryId} - for workspaceId {workspaceId}"
                );
        }
    }
}