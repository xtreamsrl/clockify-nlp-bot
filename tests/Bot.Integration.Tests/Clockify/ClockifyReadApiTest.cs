#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Services;
using Bot.Services.Clockify;
using FluentAssertions;
using Microsoft.Bot.Schema;
using Xunit;
using static Bot.Integration.Tests.Clockify.ClockifyConsts;

namespace Bot.Integration.Tests.Clockify
{
    public class ClockifyReadApiTest
    {
        // TODO Add a class Setup/Teardown to reconstruct clockify test env.
        // Unfortunately The workspace must be created by hand.
        
        [Fact]
        public async void GetClients_ApiKeyIsNotValidAndWorkspaceExist_ThrowsException()
        {
            var clockifyService = new ClockifyService();

            Func<Task> action = () => clockifyService.GetClientsAsync(InvalidApiKey, ClockifyWorkspaceId);

            await action.Should().ThrowExactlyAsync<ErrorResponseException>()
                .WithMessage($"Unable to get clients for workspaceId {ClockifyWorkspaceId}");
        }

        [Fact]
        public async void GetClients_ApiKeyIsValidAndWorkspaceDoesNotExist_ThrowsException()
        {
            var clockifyService = new ClockifyService();

            Func<Task> action = () => clockifyService.GetClientsAsync(ClockifyApiKey, NotExistingWorkspaceId);

            await action.Should().ThrowExactlyAsync<ErrorResponseException>()
                .WithMessage($"Unable to get clients for workspaceId {NotExistingWorkspaceId}");
        }
        
        [Fact]
        public async void GetTag_ApiKeyIsValid_ShouldReturnTag()
        {
            var clockifyService = new ClockifyService();
            
            string? tag = await clockifyService.GetTagAsync(ClockifyApiKey, ClockifyWorkspaceId, "bot");

            tag.Should().NotBeNullOrEmpty("tag should exist for the workspace");
        }

        [Fact]
        public async void GetTag_ApiKeyIsInvalid_ThrowException()
        {
            var clockifyService = new ClockifyService();

            Func<Task> action = () => clockifyService.GetTagAsync(InvalidApiKey, ClockifyApiKey, "bot");

            await action.Should().ThrowExactlyAsync<ErrorResponseException>().WithMessage("Unable to get tag");
        }

        [Fact]
        public async void GetClients_ApiKeyIsValidAndWorkspaceExist_ShouldReturnAllClients()
        {
            var clockifyService = new ClockifyService();

            var clients = await clockifyService.GetClientsAsync(ClockifyApiKey, ClockifyWorkspaceId);

            clients.Should().NotBeNullOrEmpty($"clients should exist for workspaceId -  {ClockifyWorkspaceId}");
        }

        [Fact]
        public async void
            GetHydratedTimeEntries_ApiKeyIsInvalid_ThrowException()
        {
            var clockifyService = new ClockifyService();

            Func<Task> action = () =>
                clockifyService.GetHydratedTimeEntriesAsync(InvalidApiKey, ClockifyWorkspaceId, "invalid");

            await action.Should().ThrowExactlyAsync<ErrorResponseException>(
                $"Unable to get time entries for workspaceId {ClockifyWorkspaceId} for user invalid");
        }

        [Fact]
        public async void
            GetHydratedTimeEntries_ApiKeyIsValidAndWorkspaceExistAndUserExist_ShouldReturnAllUserTimeEntries()
        {
            var clockifyService = new ClockifyService();

            // TODO read user from workspace
            const string existingUserId = "5efc6d24f833d7257bfa352b";

            var hydratedTimeEntries =
                await clockifyService.GetHydratedTimeEntriesAsync(ClockifyApiKey, ClockifyWorkspaceId, existingUserId);

            hydratedTimeEntries.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async void GetProjects_ApiKeyIsNotValidAndWorkspaceExist_ThrowsException()
        {
            var clockifyService = new ClockifyService();

            Func<Task> action = () => clockifyService.GetProjectsAsync(InvalidApiKey, ClockifyWorkspaceId);

            await action.Should().ThrowExactlyAsync<ErrorResponseException>()
                .WithMessage($"Unable to get projects for workspaceId {ClockifyWorkspaceId}");
        }

        [Fact]
        public async void GetProjects_ApiKeyIsValidAndWorkspaceDoesNotExist_ThrowsException()
        {
            var clockifyService = new ClockifyService();

            Func<Task> action = () => clockifyService.GetProjectsAsync(ClockifyApiKey, NotExistingWorkspaceId);

            await action.Should().ThrowExactlyAsync<ErrorResponseException>()
                .WithMessage($"Unable to get projects for workspaceId {NotExistingWorkspaceId}");
        }

        [Fact]
        public async void GetProjects_ApiKeyIsValidAndWorkspaceExist_ShouldReturnAllProjects()
        {
            var clockifyService = new ClockifyService();

            var projects = await clockifyService.GetProjectsAsync(ClockifyApiKey, ClockifyWorkspaceId);

            projects.Should().NotBeNullOrEmpty($"projects should exist for workspaceId -  {ClockifyWorkspaceId}");
        }

        [Fact]
        public async void GetProjectsByClients_ApiKeyIsValidAndWorkspaceDoesNotExistAndClientsExist_ThrowsException()
        {
            var clockifyService = new ClockifyService();

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
            var clockifyService = new ClockifyService();

            const string notExistingClientId = "invalid-client-id";

            var clients = new List<string> {notExistingClientId};

            var projects = await clockifyService.GetProjectsByClientsAsync(ClockifyApiKey, ClockifyWorkspaceId, clients);

            projects.Should().BeEmpty($"clientId {notExistingClientId} does not exist");
        }

        [Fact]
        public async void
            GetProjectsByClients_ApiKeyIsValidAndWorkspaceExistAndClientsExist_ShouldReturnAllProjectsFilteredByClients()
        {
            var clockifyService = new ClockifyService();

            // TODO read clients from workspace
            const string client1 = "5efc6e5d963f622c66c55662";
            const string client2 = "5efc6e29f833d7257bfa38f0";

            var clients = new List<string> {client1, client2};

            var projects = await clockifyService.GetProjectsByClientsAsync(ClockifyApiKey, ClockifyWorkspaceId, clients);

            projects.Should()
                .OnlyContain(project => clients.Contains(project.ClientId),
                    $"all projects found must contain one of the following clients - {clients}");
        }

        [Fact]
        public async void GetTasks_ApiKeyIsValidAndWorkspaceExistAndProjectDoesNotExist_ThrowsException()
        {
            var clockifyService = new ClockifyService();

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
            var clockifyService = new ClockifyService();

            // TODO read project id from api
            const string projectId = "5efc6ee1963f622c66c55819";

            var tasks = await clockifyService.GetTasksAsync(ClockifyApiKey, ClockifyWorkspaceId, projectId);

            tasks.Should().NotBeNullOrEmpty($"project {projectId} contains tasks");
        }

        [Fact]
        public async void GetUser_ApiKeyIsInvalid_ThrowException()
        {
            var clockifyService = new ClockifyService();

            Func<Task> action = () => clockifyService.GetCurrentUserAsync(InvalidApiKey);

            await action.Should().ThrowExactlyAsync<ErrorResponseException>()
                .WithMessage("Unable to get current user");
        }

        [Fact]
        public async void GetUser_ApiKeyIsValid_ShouldReturnCurrentUser()
        {
            var clockifyService = new ClockifyService();

            var currentUser = await clockifyService.GetCurrentUserAsync(ClockifyApiKey);

            currentUser.Id.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GetWorkspaces_ApiKeyIsInvalid_ThrowsException()
        {
            // arrange
            var clockifyService = new ClockifyService();

            // act
            Func<Task> action = async () => await clockifyService.GetWorkspacesAsync(InvalidApiKey);

            // assert
            action.Should().ThrowExactly<ErrorResponseException>().WithMessage("Unable to get workspaces");
        }

        [Fact]
        public async void GetWorkspaces_ApiKeyIsValid_ShouldReturnAllWorkspaces()
        {
            var clockifyService = new ClockifyService();

            var workspaces = await clockifyService.GetWorkspacesAsync(ClockifyApiKey);

            workspaces.Should().NotBeNullOrEmpty("workspaces should exist for the user");
        }
    }
}