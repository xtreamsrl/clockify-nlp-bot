using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bot.Clockify.Models;
using Microsoft.Bot.Schema;
using RestSharp;

namespace Bot.Clockify.Client
{
    // Read operations
    public partial class ClockifyService : IClockifyService
    {
        private const int PageSize = 2000;
        private readonly IClockifyClientFactory _clockifyClientFactory;

        public ClockifyService(IClockifyClientFactory clockifyClientFactory)
        {
            _clockifyClientFactory = clockifyClientFactory;
        }

        public async Task<UserDo> GetCurrentUserAsync(string apiKey)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.GetCurrentUserAsync();
            ThrowUnauthorizedIf401(response);
            if (!response.IsSuccessful) throw new ErrorResponseException("Unable to get current user");

            return ClockifyModelFactory.ToUserDo(response.Data);
        }

        public async Task<List<WorkspaceDo>> GetWorkspacesAsync(string apiKey)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.GetWorkspacesAsync();
            ThrowUnauthorizedIf401(response);
            if (!response.IsSuccessful) throw new ErrorResponseException("Unable to get workspaces");

            return response.Data.Select(ClockifyModelFactory.ToWorkspaceDo).ToList();
        }

        public async Task<List<ClientDo>> GetClientsAsync(string apiKey, string workspaceId)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.FindAllClientsOnWorkspaceAsync(workspaceId);
            ThrowUnauthorizedIf401(response);
            if (!response.IsSuccessful)
                throw new ErrorResponseException($"Unable to get clients for workspaceId {workspaceId}");

            return response.Data.Select(ClockifyModelFactory.ToClientDo).ToList();
        }

        public async Task<List<ProjectDo>> GetProjectsAsync(string apiKey,
            string workspaceId)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            // TODO Implement pagination? Clockify api do not put any total page in response body
            var response = await clockifyClient.FindAllProjectsOnWorkspaceAsync(workspaceId, 1, PageSize);
            ThrowUnauthorizedIf401(response);
            if (!response.IsSuccessful)
                throw new ErrorResponseException($"Unable to get projects for workspaceId {workspaceId}");

            return response.Data.Select(ClockifyModelFactory.ToProjectDo).ToList();
        }

        public async Task<List<ProjectDo>> GetProjectsByClientsAsync(string apiKey,
            string workspaceId, IEnumerable<string> clients)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response = await clockifyClient.FindAllProjectsOnWorkspaceByClientsAsync(workspaceId, clients);
            ThrowUnauthorizedIf401(response);
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
            ThrowUnauthorizedIf401(response);
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
            ThrowUnauthorizedIf401(response);
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
            ThrowUnauthorizedIf401(response);
            if (!response.IsSuccessful)
            {
                throw new ErrorResponseException("Unable to get tag");
            }

            return response.Data.Find(e => e.Name == tagName)?.Id;
        }

        public async Task<TaskDo> CreateTaskAsync(string apiKey, TaskReq taskReq, string projectId, string workspaceId)
        {
            var clockifyClient = _clockifyClientFactory.CreateClient(apiKey);
            var response =
                await clockifyClient.CreateTaskAsync(workspaceId, projectId,
                    ClockifyModelFactory.ToTaskRequest(taskReq));
            ThrowUnauthorizedIf401(response);
            if (!response.IsSuccessful)
                throw new ErrorResponseException(
                    $"Unable to create task for workspaceId {workspaceId} and projectId {projectId}"
                );
            return ClockifyModelFactory.ToTaskDo(response.Data);
        }

        private static void ThrowUnauthorizedIf401(IRestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException();
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
            ThrowUnauthorizedIf401(response);
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
            ThrowUnauthorizedIf401(response);
            if (!response.IsSuccessful)
                throw new ErrorResponseException(
                    $"Unable to delete time entry - {timeEntryId} - for workspaceId {workspaceId}"
                );
        }
    }
}