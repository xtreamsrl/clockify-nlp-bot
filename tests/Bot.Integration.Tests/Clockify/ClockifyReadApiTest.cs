#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Integration.Tests.Clockify.Supports;
using FluentAssertions;
using Microsoft.Bot.Schema;
using Xunit;
using static Bot.Integration.Tests.Clockify.ClockifyConsts;

namespace Bot.Integration.Tests.Clockify
{
    public class ClockifyReadApiTest : IClassFixture<ClockifyFixture>
    {
        private readonly ClockifyFixture _clockifyFixture;

        public ClockifyReadApiTest(ClockifyFixture clockifyFixture)
        {
            _clockifyFixture = clockifyFixture;
        }

        [Fact]
        public async void GetClients_ApiKeyIsNotValidAndWorkspaceExist_ThrowsException()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            Func<Task> action = () => clockifyService.GetClientsAsync(InvalidApiKey, ClockifyWorkspaceId);

            await action.Should().ThrowExactlyAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async void GetClients_ApiKeyIsValidAndWorkspaceDoesNotExist_ThrowsException()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            Func<Task> action = () => clockifyService.GetClientsAsync(ClockifyApiKey, NotExistingWorkspaceId);

            await action.Should().ThrowExactlyAsync<ErrorResponseException>()
                .WithMessage($"Unable to get clients for workspaceId {NotExistingWorkspaceId}");
        }

        [Fact]
        public async void GetClients_ApiKeyIsValidAndWorkspaceExist_ShouldReturnAllClients()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            var clients = await clockifyService.GetClientsAsync(ClockifyApiKey, ClockifyWorkspaceId);

            clients.Should().NotBeNullOrEmpty($"clients should exist for workspaceId -  {ClockifyWorkspaceId}");
        }

        // TODO populate tag.
        [Fact]
        public async void GetTag_ApiKeyIsValid_ShouldReturnTag()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            string tagName = _clockifyFixture.BotTag().Name;

            string? tag = await clockifyService.GetTagAsync(ClockifyApiKey, ClockifyWorkspaceId, tagName);

            tag.Should().NotBeNullOrEmpty("tag should exist for the workspace");
        }

        [Fact]
        public async void GetTag_ApiKeyIsInvalid_ThrowException()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            Func<Task> action = () => clockifyService.GetTagAsync(InvalidApiKey, ClockifyApiKey, "bot");

            await action.Should().ThrowExactlyAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async void
            GetHydratedTimeEntries_ApiKeyIsInvalid_ThrowException()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            Func<Task> action = () =>
                clockifyService.GetHydratedTimeEntriesAsync(InvalidApiKey, ClockifyWorkspaceId, "invalid");

            await action.Should().ThrowExactlyAsync<UnauthorizedAccessException>(
                $"Unable to get time entries for workspaceId {ClockifyWorkspaceId} for user invalid");
        }

        [Fact]
        public async void
            GetHydratedTimeEntries_ApiKeyIsValidAndWorkspaceExistAndUserExist_ShouldReturnAllUserTimeEntries()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());
            
            string existingUserId = clockifyService.GetCurrentUserAsync(ClockifyApiKey).Result.Id;

            var hydratedTimeEntries =
                await clockifyService.GetHydratedTimeEntriesAsync(ClockifyApiKey, ClockifyWorkspaceId, existingUserId);

            hydratedTimeEntries.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async void GetProjects_ApiKeyIsNotValidAndWorkspaceExist_ThrowsException()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            Func<Task> action = () => clockifyService.GetProjectsAsync(InvalidApiKey, ClockifyWorkspaceId);

            await action.Should().ThrowExactlyAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async void GetProjects_ApiKeyIsValidAndWorkspaceDoesNotExist_ThrowsException()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            Func<Task> action = () => clockifyService.GetProjectsAsync(ClockifyApiKey, NotExistingWorkspaceId);

            await action.Should().ThrowExactlyAsync<ErrorResponseException>()
                .WithMessage($"Unable to get projects for workspaceId {NotExistingWorkspaceId}");
        }

        [Fact]
        public async void GetProjects_ApiKeyIsValidAndWorkspaceExist_ShouldReturnAllProjects()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            var projects = await clockifyService.GetProjectsAsync(ClockifyApiKey, ClockifyWorkspaceId);

            projects.Should().NotBeNullOrEmpty($"projects should exist for workspaceId -  {ClockifyWorkspaceId}");
        }

        [Fact]
        public async void GetProjectsByClients_ApiKeyIsValidAndWorkspaceDoesNotExistAndClientsExist_ThrowsException()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            const string client = "5efc6e5d963f622c66c55662";

            var clients = new List<string> {client};

            Func<Task> action = () =>
                clockifyService.GetProjectsByClientsAsync(ClockifyApiKey, NotExistingWorkspaceId, clients);

            await action.Should().ThrowExactlyAsync<ErrorResponseException>()
                .WithMessage($"Unable to get projects for workspaceId {NotExistingWorkspaceId} and clients {clients}");
        }

        [Fact]
        public async void
            GetProjectsByClients_ApiKeyIsValidAndWorkspaceExistAndClientNotExist_ShouldReturnZeroProjects()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            var clients = new List<string> {InvalidClientId};

            var projects = await clockifyService.GetProjectsByClientsAsync(ClockifyApiKey, ClockifyWorkspaceId, clients);

            projects.Should().BeEmpty($"clientId {InvalidClientId} does not exist");
        }

        [Fact]
        public async void
            GetProjectsByClients_ApiKeyIsValidAndWorkspaceExistAndClientsExist_ShouldReturnAllProjectsFilteredByClients()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            var clientIds = _clockifyFixture.Clients().Select(c => c.Id).ToList();

            var projects = await clockifyService.GetProjectsByClientsAsync(ClockifyApiKey, ClockifyWorkspaceId, clientIds);

            projects.Should()
                .OnlyContain(project => clientIds.Contains(project.ClientId),
                    $"all projects found must contain one of the following clients - {clientIds}");
        }

        [Fact]
        public async void GetTasks_ApiKeyIsValidAndWorkspaceExistAndProjectDoesNotExist_ThrowsException()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            const string notExistingProjectId = "invalid-project-id";

            Func<Task> action = () =>
                clockifyService.GetTasksAsync(ClockifyApiKey, ClockifyWorkspaceId, notExistingProjectId);

            await action.Should().ThrowExactlyAsync<ErrorResponseException>()
                .WithMessage(
                    $"Unable to get tasks for workspaceId {ClockifyWorkspaceId} and projectId {notExistingProjectId}");
        }

        [Fact]
        public async void GetTasks_ApiKeyIsValidAndWorkspaceExistAndProjectExist_ShouldReturnAllProjectTasks()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());
            
            string projectId = _clockifyFixture.ProjectWithTasks().Id;

            var tasks = await clockifyService.GetTasksAsync(ClockifyApiKey, ClockifyWorkspaceId, projectId);

            tasks.Should().NotBeNullOrEmpty($"project {projectId} contains tasks");
        }

        [Fact]
        public async void GetUser_ApiKeyIsInvalid_ThrowException()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            Func<Task> action = () => clockifyService.GetCurrentUserAsync(InvalidApiKey);

            await action.Should().ThrowExactlyAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async void GetUser_ApiKeyIsValid_ShouldReturnCurrentUser()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            var currentUser = await clockifyService.GetCurrentUserAsync(ClockifyApiKey);

            currentUser.Id.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GetWorkspaces_ApiKeyIsInvalid_ThrowsException()
        {
            // arrange
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            // act
            Func<Task> action = async () => await clockifyService.GetWorkspacesAsync(InvalidApiKey);

            // assert
            action.Should().ThrowExactly<UnauthorizedAccessException>();
        }

        [Fact]
        public async void GetWorkspaces_ApiKeyIsValid_ShouldReturnAllWorkspaces()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());

            var workspaces = await clockifyService.GetWorkspacesAsync(ClockifyApiKey);

            workspaces.Should().NotBeNullOrEmpty("workspaces should exist for the user");
        }
    }
}