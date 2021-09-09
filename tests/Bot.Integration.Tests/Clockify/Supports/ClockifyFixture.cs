using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Clockify.Models;
using Xunit;
using static Bot.Integration.Tests.Clockify.ClockifyConsts;

namespace Bot.Integration.Tests.Clockify.Supports
{
    /// <summary>
    /// Setup and cleanup clockify environment. If something goes wrong you need to manually sanitize your clockify
    /// workspace by deleting it and recreating a new one.
    ///
    /// Please use this fixture only with <see cref="ClockifyCollection">MyClass</see>. Multiple instances will result
    /// in test failure due to entity creation name conflicts.
    /// </summary>
    public class ClockifyFixture : IAsyncLifetime
    {
        private readonly TestClockifyClient _testClockifyClient = new TestClockifyClient();

        private List<ClientDo> _clients;
        private List<ProjectDo> _projects;
        private List<TaskDo> _tasks;
        private TagDo _tag;
        private List<TimeEntryDo> _timeEntries;

        private const string ClientA = nameof(ClientA);
        private const string ClientB = nameof(ClientB);
        private const string ProjectA = nameof(ProjectA);
        private const string ProjectB = nameof(ProjectB);
        private const string TaskA = nameof(TaskA);
        private const string Tag = nameof(Tag);

        public async Task InitializeAsync()
        {
            _tag = await CreateBotTag();
            _clients = await SetupClients();
            _projects = await SetupProjects();
            _tasks = await SetupTasks();
            _timeEntries = await AddTimeEntries();
        }

        public async Task DisposeAsync()
        {
            await CleanupTimeEntries(_timeEntries);
            await CleanupTasks(_tasks);
            await CleanupProjects(_projects);
            await CleanupClients(_clients);
            await CleanupBotTag(_tag);
        }

        public IEnumerable<ClientDo> Clients() => _clients;

        public ProjectDo ProjectWithTasks() => _projects[0];

        public ProjectDo ProjectWithoutTasks() => _projects[1];

        public TagDo BotTag() => _tag;

        private async Task<List<ClientDo>> SetupClients()
        {
            var clientA = await _testClockifyClient.CreateClientAsync(ClockifyWorkspaceId, new ClientReq(ClientA));
            var clientB = await _testClockifyClient.CreateClientAsync(ClockifyWorkspaceId, new ClientReq(ClientB));
            return new List<ClientDo> { clientA, clientB };
        }

        private async Task CleanupClients(List<ClientDo> clients)
        {
            foreach (var client in clients)
            {
                await _testClockifyClient.DeleteClientAsync(ClockifyWorkspaceId, client.Id);
            }
        }

        private async Task<List<ProjectDo>> SetupProjects()
        {
            var projectReqA = new ProjectReq(ProjectA)
            {
                ClientId = _clients[0].Id
            };
            var projectReqB = new ProjectReq(ProjectB)
            {
                ClientId = _clients[1].Id
            };
            var projectWithTasks =
                await _testClockifyClient.CreateProjectAsync(ClockifyWorkspaceId, projectReqA);
            var projectWithoutTasks =
                await _testClockifyClient.CreateProjectAsync(ClockifyWorkspaceId, projectReqB);

            return new List<ProjectDo> { projectWithTasks, projectWithoutTasks };
        }

        private async Task CleanupProjects(List<ProjectDo> projects)
        {
            foreach (var project in projects)
            {
                await _testClockifyClient.ArchiveProjectAsync(project);
                await _testClockifyClient.DeleteProjectAsync(project.WorkspaceId, project.Id);
            }
        }

        private async Task<List<TaskDo>> SetupTasks()
        {
            var task = await _testClockifyClient.CreateTaskAsync(ProjectWithTasks().WorkspaceId, ProjectWithTasks().Id,
                new TaskReq(TaskA));
            return new List<TaskDo> { task };
        }

        private async Task CleanupTasks(List<TaskDo> tasks)
        {
            foreach (var task in tasks)
            {
                await _testClockifyClient.DeleteTaskAsync(ClockifyWorkspaceId, task.ProjectId, task.Id);
            }
        }

        private async Task<TagDo> CreateBotTag()
        {
            return await _testClockifyClient.CreateTagAsync(ClockifyWorkspaceId, Tag);
        }

        private async Task CleanupBotTag(TagDo tag)
        {
            await _testClockifyClient.DeleteTagAsync(ClockifyWorkspaceId, tag.Id);
        }

        private async Task<List<TimeEntryDo>> AddTimeEntries()
        {
            var now = DateTimeOffset.UtcNow;
            var timeEntryReq = new TimeEntryReq(ProjectWithTasks().Id, now, end: now.AddHours(4));
            var timeEntry = await _testClockifyClient.CreateTimeEntryAsync(ClockifyWorkspaceId, timeEntryReq);
            return new List<TimeEntryDo> { timeEntry };
        }

        private async Task CleanupTimeEntries(List<TimeEntryDo> timeEntries)
        {
            foreach (var timeEntry in timeEntries)
            {
                await _testClockifyClient.DeleteTimeEntryAsync(ClockifyWorkspaceId, timeEntry.Id);
            }
        }
    }
}