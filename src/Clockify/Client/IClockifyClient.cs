using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clockify.Net.Models.Clients;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Tags;
using Clockify.Net.Models.Tasks;
using Clockify.Net.Models.Templates;
using Clockify.Net.Models.TimeEntries;
using Clockify.Net.Models.Users;
using Clockify.Net.Models.Workspaces;
using RestSharp;

namespace Bot.Clockify.Client
{
    public interface IClockifyClient
    {
        Task<IRestResponse<List<TaskDto>>> FindAllTasksAsync(
            string workspaceId,
            string projectId,
            bool? isActive = null,
            string? name = null,
            int page = 1,
            int pageSize = 50);

        Task<IRestResponse<TaskDto>> CreateTaskAsync(string workspaceId, string projectId, TaskRequest taskRequest);

        Task<IRestResponse<List<UserDto>>> FindAllUsersOnWorkspaceAsync(string workspaceId);

        Task<IRestResponse<CurrentUserDto>> GetCurrentUserAsync();

        Task<IRestResponse<UserDto>> SetActiveWorkspaceFor(string userId, string workspaceId);

        Task<IRestResponse<List<WorkspaceDto>>> GetWorkspacesAsync();

        Task<IRestResponse<WorkspaceDto>> CreateWorkspaceAsync(WorkspaceRequest workspaceRequest);

        Task<IRestResponse> DeleteWorkspaceAsync(string id);

        Task<IRestResponse<List<ClientDto>>> FindAllClientsOnWorkspaceAsync(string workspaceId);

        Task<IRestResponse<ClientDto>> CreateClientAsync(string workspaceId, ClientRequest clientRequest);

        Task<IRestResponse<List<ProjectDtoImpl>>> FindAllProjectsOnWorkspaceAsync(
            string workspaceId,
            bool? isActive = null,
            int page = 1,
            int pageSize = 50);

        Task<IRestResponse<ProjectDtoImpl>> CreateProjectAsync(string workspaceId, ProjectRequest projectRequest);

        Task<IRestResponse> DeleteProjectAsync(string workspaceId, string id);

        Task<IRestResponse<List<TagDto>>> FindAllTagsOnWorkspaceAsync(string workspaceId);

        Task<IRestResponse<TagDto>> CreateTagAsync(string workspaceId, TagRequest projectRequest);

        Task<IRestResponse<List<TemplateDto>>> FindAllTemplatesOnWorkspaceAsync(
            string workspaceId,
            string? name = null,
            bool cleansed = false,
            bool hydrated = false,
            int page = 1,
            int pageSize = 1);

        Task<IRestResponse<TemplateDto>> GetTemplateAsync(
            string workspaceId,
            string templateId,
            bool cleansed = false,
            bool hydrated = false);

        Task<IRestResponse<List<TemplateDto>>> CreateTemplatesAsync(string workspaceId,
            params TemplateRequest[] projectRequests);

        Task<IRestResponse<TemplateDto>> DeleteTemplateAsync(string workspaceId, string templateId);

        Task<IRestResponse<TemplateDto>> UpdateTemplateAsync(string workspaceId, string timeEntryId,
            TemplatePatchRequest templatePatchRequest);

        Task<IRestResponse<TimeEntryDtoImpl>> CreateTimeEntryAsync(string workspaceId,
            TimeEntryRequest timeEntryRequest);

        Task<IRestResponse<TimeEntryDtoImpl>> GetTimeEntryAsync(
            string workspaceId,
            string timeEntryId,
            bool considerDurationFormat = false,
            bool hydrated = false);

        Task<IRestResponse<TimeEntryDtoImpl>> UpdateTimeEntryAsync(string workspaceId, string timeEntryId,
            UpdateTimeEntryRequest updateTimeEntryRequest);

        Task<IRestResponse> DeleteTimeEntryAsync(string workspaceId, string timeEntryId);

        Task<IRestResponse<List<TimeEntryDtoImpl>>> FindAllTimeEntriesForUserAsync(
            string workspaceId,
            string userId,
            string? description = null,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string? project = null,
            string? task = null,
            bool? projectRequired = null,
            bool? taskRequired = null,
            bool? considerDurationFormat = null,
            bool? hydrated = null,
            bool? inProgress = null,
            int page = 1,
            int pageSize = 50);

        public Task<IRestResponse<List<ProjectDtoImpl>>> FindAllProjectsOnWorkspaceByClientsAsync(string workspaceId,
            IEnumerable<string> clients);

        public Task<IRestResponse<List<HydratedTimeEntryDtoImpl>>> FindAllHydratedTimeEntriesForUserAsync(
            string workspaceId,
            string userId,
            string? description = null,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string? project = null,
            string? task = null,
            bool? projectRequired = null,
            bool? taskRequired = null,
            bool? considerDurationFormat = null,
            bool? inProgress = null,
            int page = 1,
            int pageSize = 50);
    }
}