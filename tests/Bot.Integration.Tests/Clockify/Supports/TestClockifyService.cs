using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Clockify.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using static Bot.Integration.Tests.Clockify.ClockifyConsts;

namespace Bot.Integration.Tests.Clockify.Supports
{
    public class TestClockifyService : ClockifyService
    {
        private const string BaseUrl = "https://api.clockify.me/api/v1";
        private const string ApiKeyHeaderName = "X-Api-Key";
        private readonly IRestClient _client = GetClockifyRestClient();

        public TestClockifyService(IClockifyClientFactory clockifyClientFactory) : base(clockifyClientFactory)
        {
        }

        public async Task<ClientDo> CreateClientAsync(string workspaceId, ClientReq clientReq)
        {
            var request = new RestRequest($"workspaces/{workspaceId}/clients", Method.POST);
            request.AddJsonBody(clientReq);
            var response = await _client.ExecutePostAsync<ClientDo>(request);
            ThrowOnFailure(response);
            return response.Data;
        }

        public async Task DeleteClientAsync(string workspaceId, string clientId)
        {
            var request = new RestRequest($"workspaces/{workspaceId}/clients/{clientId}", Method.DELETE);
            var response = await _client.ExecuteAsync(request);
            ThrowOnFailure(response);
        }

        public async Task<ProjectDo> CreateProjectAsync(string workspaceId, ProjectReq projectReq)
        {
            var request = new RestRequest($"workspaces/{workspaceId}/projects", Method.POST);
            request.AddJsonBody(projectReq);
            var response = await _client.ExecutePostAsync<ProjectDo>(request);
            ThrowOnFailure(response);
            return response.Data;
        }

        public async Task<ProjectDo> ArchiveProjectAsync(ProjectDo project)
        {
            var request = new RestRequest($"workspaces/{project.WorkspaceId}/projects/{project.Id}");
            request.AddJsonBody(new ArchiveProjectReq(true));
            var response = await _client.ExecuteAsync<ProjectDo>(request, Method.PUT);
            ThrowOnFailure(response);
            return response.Data;
        }

        public async Task DeleteProjectAsync(string workspaceId, string projectId)
        {
            var request = new RestRequest($"workspaces/{workspaceId}/projects/{projectId}", Method.DELETE);
            var response = await _client.ExecuteAsync(request);
            ThrowOnFailure(response);
        }
        
        public async Task<TaskDo> CreateTaskAsync(string workspaceId, string projectId, TaskReq taskReq)
        {
            var request = new RestRequest($"workspaces/{workspaceId}/projects/{projectId}/tasks", Method.POST);
            request.AddJsonBody(taskReq);
            var response = await _client.ExecutePostAsync<TaskDo>(request);
            ThrowOnFailure(response);
            return response.Data;
        }

        public async Task DeleteTaskAsync(string workspaceId, string projectId, string taskId)
        {
            var request = new RestRequest($"workspaces/{workspaceId}/projects/{projectId}/tasks/{taskId}", Method.DELETE);
            var response = await _client.ExecuteAsync(request);
            ThrowOnFailure(response);
        }

        public async Task<TagDo> CreateTagAsync(string workspaceId, string tagName)
        {
            var request = new RestRequest($"workspaces/{workspaceId}/tags", Method.POST);
            request.AddJsonBody(new Dictionary<string, string> { { "name", tagName } });
            var response = await _client.ExecutePostAsync<TagDo>(request);
            ThrowOnFailure(response);
            return response.Data;
        }

        public async Task DeleteTagAsync(string workspaceId, string tagId)
        {
            var request = new RestRequest($"workspaces/{workspaceId}/tags/{tagId}", Method.DELETE);
            var response = await _client.ExecuteAsync(request);
            ThrowOnFailure(response);
        }

        private static void ThrowOnFailure(IRestResponse response)
        {
            if (!response.IsSuccessful)
            {
                throw new Exception(
                    $"Clockify request has failed with status {response.StatusCode}. Sanitize your clockify workspace if necessary.");
            }
        }

        private static IRestClient GetClockifyRestClient()
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

            var client = new RestClient(BaseUrl);
            client.AddDefaultHeader(ApiKeyHeaderName, ClockifyApiKey);
            client.UseNewtonsoftJson(jsonSerializerSettings);

            return client;
        }
    }
}