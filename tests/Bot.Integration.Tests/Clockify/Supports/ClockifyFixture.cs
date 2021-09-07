using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Clockify.Models;
using Xunit;
using static Bot.Integration.Tests.Clockify.ClockifyConsts;

namespace Bot.Integration.Tests.Clockify.Supports
{
    public class ClockifyFixture : IAsyncLifetime
    {
        private readonly TestClockifyService _testClockifyService = new TestClockifyService(new ClockifyClientFactory());
        
        private List<ClientDo> _clients;

        public async Task InitializeAsync()
        {
            _clients = await SetupClients();
        }

        public async Task DisposeAsync()
        {
            await CleanupClients(_clients);
        }

        private async Task<List<ClientDo>> SetupClients()
        {
            var client1 = await _testClockifyService.CreateClientAsync(ClockifyWorkspaceId, new ClientReq("Client1"));
            var client2 = await _testClockifyService.CreateClientAsync(ClockifyWorkspaceId, new ClientReq("Client2"));

            return new List<ClientDo> { client1, client2 };
        }

        private async Task CleanupClients(List<ClientDo> clients)
        {
            foreach (var client in clients)
            {
                await _testClockifyService.DeleteClientAsync(ClockifyWorkspaceId, client.Id);
            }
        }
    }
}