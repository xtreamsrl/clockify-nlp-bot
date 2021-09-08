using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Bot.Clockify.Client;
using Bot.Clockify.Models;
using Xunit;
using static Bot.Integration.Tests.Clockify.ClockifyConsts;

namespace Bot.Integration.Tests.Clockify.Supports
{
    public class ClockifyFixture : IAsyncLifetime
    {
        private readonly TestClockifyService _testClockifyService = new TestClockifyService(new ClockifyClientFactory());
        private Fixture _fixture = new Fixture();
        
        private List<ClientDo> _clients;
        private List<ProjectDo> _projects;

        public async Task InitializeAsync()
        {
            _clients = await SetupClients();
            _projects = await SetupProjects();
        }

        public async Task DisposeAsync()
        {
            // TODO Maybe it's better to cleanup everything and not only entities created in the current test run.
            await CleanupProjects(_projects);
            await CleanupClients(_clients);
        }

        public IEnumerable<ClientDo> Clients() => _clients;

        private async Task<List<ClientDo>> SetupClients()
        {
            var client1 = await _testClockifyService.CreateClientAsync(ClockifyWorkspaceId, _fixture.Create<ClientReq>());
            var client2 = await _testClockifyService.CreateClientAsync(ClockifyWorkspaceId, _fixture.Create<ClientReq>());

            return new List<ClientDo> { client1, client2 };
        }

        private async Task CleanupClients(List<ClientDo> clients)
        {
            foreach (var client in clients)
            {
                await _testClockifyService.DeleteClientAsync(ClockifyWorkspaceId, client.Id);
            }
        }

        private async Task<List<ProjectDo>> SetupProjects()
        {
            var projectReq1 = new ProjectReq(_fixture.Create<string>())
            {
                ClientId = _clients[0].Id
            };
            var projectReq2 = new ProjectReq(_fixture.Create<string>())
            {
                ClientId = _clients[1].Id
            };
            var project1 =
                await _testClockifyService.CreateProjectAsync(ClockifyWorkspaceId, projectReq1);
            var project2 =
                await _testClockifyService.CreateProjectAsync(ClockifyWorkspaceId, projectReq2);

            return new List<ProjectDo> { project1, project2 };
        }
        
        private async Task CleanupProjects(List<ProjectDo> projects)
        {
            foreach (var project in projects)
            {
                await _testClockifyService.ArchiveProjectAsync(project);
                await _testClockifyService.DeleteProjectAsync(project.WorkspaceId, project.Id);
            }
        }
    }
}