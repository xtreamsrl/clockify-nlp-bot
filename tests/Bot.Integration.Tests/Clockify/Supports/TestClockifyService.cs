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

        public Task DeleteClientAsync(string workspaceId, string clientId)
        {
            var request = new RestRequest($"workspaces/{workspaceId}/clients/{clientId}", Method.DELETE);
            return _client.ExecuteAsync(request);
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