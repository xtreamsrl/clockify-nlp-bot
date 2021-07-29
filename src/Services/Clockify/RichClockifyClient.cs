using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clockify.Net;
using Clockify.Net.Models.Clients;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Tags;
using Clockify.Net.Models.Tasks;
using Clockify.Net.Models.TimeEntries;
using Clockify.Net.Models.Users;
using Clockify.Net.Models.Workspaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Bot.Services.Clockify
{
    public class RichClockifyClient : IClockifyClient
    {
        private const string BaseUrl = "https://api.clockify.me/api/v1";
        private const string ApiKeyHeaderName = "X-Api-Key";

        private IRestClient _client = null!;
        private readonly ClockifyClient _clockifyClient;

        public RichClockifyClient(string apiKey)
        {
            InitRestClient(apiKey);
            _clockifyClient = new ClockifyClient(apiKey);
        }

        public Task<IRestResponse> DeleteTimeEntryAsync(string workspaceId, string timeEntryId)
        {
            return _clockifyClient.DeleteTimeEntryAsync(workspaceId, timeEntryId);
        }

        public Task<IRestResponse<List<ProjectDtoImpl>>> FindAllProjectsOnWorkspaceByClientsAsync(
            string workspaceId, IEnumerable<string> clients)
        {
            var restRequest = new RestRequest("workspaces/" + workspaceId + "/projects");
            restRequest.AddQueryParameter("clients", string.Join(",", clients));

            return _client.ExecuteGetAsync<List<ProjectDtoImpl>>(
                restRequest);
        }

        public Task<IRestResponse<List<HydratedTimeEntryDtoImpl>>> FindAllHydratedTimeEntriesForUserAsync(string workspaceId, string userId, string? description = null,
            DateTimeOffset? start = null, DateTimeOffset? end = null, string? project = null, string? task = null,
            bool? projectRequired = null, bool? taskRequired = null, bool? considerDurationFormat = null,
            bool? inProgress = null, int page = 1, int pageSize = 50)
        {
            return _clockifyClient.FindAllHydratedTimeEntriesForUserAsync(workspaceId, userId, description, start, end, project, task, projectRequired, taskRequired, considerDurationFormat, inProgress, page, pageSize);
        }

        public Task<IRestResponse<List<TaskDto>>> FindAllTasksAsync(string workspaceId, string projectId, bool? isActive = null, string? name = null, int page = 1,
            int pageSize = 50)
        {
            return _clockifyClient.FindAllTasksAsync(workspaceId, projectId, isActive, name, page, pageSize);
        }

        public Task<IRestResponse<TaskDto>> CreateTaskAsync(string workspaceId, string projectId, TaskRequest taskRequest)
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
            bool? isActive = null,
            int page = 1,
            int pageSize = 50)
        {
            var restRequest = new RestRequest("workspaces/" + workspaceId + "/projects");
            if (isActive.HasValue)
                restRequest.AddQueryParameter("is-active", isActive.ToString()!);
            restRequest.AddQueryParameter("page", page.ToString());
            restRequest.AddQueryParameter("page-size", pageSize.ToString());
            return _client.ExecuteGetAsync<List<ProjectDtoImpl>>(restRequest);
        }

        public Task<IRestResponse<List<TagDto>>> FindAllTagsOnWorkspaceAsync(string workspaceId)
        {
            return _clockifyClient.FindAllTagsOnWorkspaceAsync(workspaceId);
        }

        public Task<IRestResponse<TimeEntryDtoImpl>> CreateTimeEntryAsync(string workspaceId, TimeEntryRequest timeEntryRequest)
        {
            return _clockifyClient.CreateTimeEntryAsync(workspaceId, timeEntryRequest);
        }

        private void InitRestClient(string apiKey)
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(),
                    new IsoDateTimeConverter()
                    {
                        DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"
                    }
                },
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            
            _client = new RestClient(BaseUrl);
            _client.AddDefaultHeader(ApiKeyHeaderName, apiKey);
            _client.UseNewtonsoftJson(jsonSerializerSettings);
        }
    }
}