using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Factories;
using Clockify.Net;
using Clockify.Net.Models.Projects;
using RestSharp;

namespace Bot.Services
{
    public class RichClockifyClient : ClockifyClient, IClockifyClient
    {
        private const string BaseUrl = "https://api.clockify.me/api/v1";
        private const string ApiKeyHeaderName = "X-Api-Key";

        private IRestClient _client = null!;

        public RichClockifyClient(string apiKey) : base(apiKey)
        {
            InitClients(apiKey);
        }

        public Task<IRestResponse<List<ProjectDtoImpl>>> FindAllProjectsOnWorkspaceByClientsAsync(
            string workspaceId, IEnumerable<string> clients)
        {
            var restRequest = new RestRequest("workspaces/" + workspaceId + "/projects");
            restRequest.AddQueryParameter("clients", string.Join(",", clients));

            return _client.ExecuteGetAsync<List<ProjectDtoImpl>>(
                restRequest);
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
        
        private void InitClients(string apiKey)
        {
            _client = new RestClient(BaseUrl);
            _client.AddDefaultHeader(ApiKeyHeaderName, apiKey);
            SimpleJson.CurrentJsonSerializerStrategy = new PocoJsonSerializerStrategy();
        }
    }
}