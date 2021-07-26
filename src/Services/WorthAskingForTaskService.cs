using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.States;
using Castle.Core.Internal;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.TimeEntries;

namespace Bot.Services
{
    public class WorthAskingForTaskService
    {
        private readonly IClockifyService _clockifyService;

        public WorthAskingForTaskService(IClockifyService clockifyService)
        {
            _clockifyService = clockifyService;
        }

        public async Task<bool> IsWorthAskingForTask(ProjectDtoImpl project, UserProfile user)
        {
            string clockifyToken = user.ClockifyToken ?? throw new ArgumentNullException(nameof(user.ClockifyToken));
            string userId = user.UserId ?? throw new ArgumentNullException(nameof(user.UserId));
            var associatedTasks = await _clockifyService.GetTasksAsync(clockifyToken, project.WorkspaceId, project.Id);
            if (associatedTasks.IsNullOrEmpty()) return false;
            var end = DateTimeOffset.Now;
            var start = end.AddDays(-90);
            List<HydratedTimeEntryDtoImpl> history = await _clockifyService.GetHydratedTimeEntriesAsync(
                clockifyToken,
                project.WorkspaceId,
                userId,
                start,
                end);
            history = history.Where(e => e.ProjectId == project.Id).ToList();
            int totalHistorySize = history.Count;
            int historySizeWithTaskPopulated = history.Count(e => e.Task != null);
            bool thereIsEnoughHistory = totalHistorySize > 5;
            bool tasksAreUsuallySetOnThisProject = historySizeWithTaskPopulated >= 0.3 * totalHistorySize;
            return !thereIsEnoughHistory || tasksAreUsuallySetOnThisProject;
        }
    }
}