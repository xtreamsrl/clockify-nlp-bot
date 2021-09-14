using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Clockify.Models;
using Microsoft.Extensions.Configuration;

namespace Bot.Clockify.Fill
{
    public class TimeEntryStoreService : ITimeEntryStoreService
    {
        private readonly IClockifyService _clockifyService;
        private readonly string _tagName;

        public TimeEntryStoreService(IClockifyService clockifyService, IConfiguration configuration)
        {
            _clockifyService = clockifyService;
            _tagName = configuration["Tag"];
        }

        public async Task<double> AddTimeEntries(string clockifyToken, ProjectDo project, TaskDo? task, double minutes)
        {
            string? tagId = await _clockifyService.GetTagAsync(clockifyToken, project.WorkspaceId, _tagName);
            string userId = (await _clockifyService.GetCurrentUserAsync(clockifyToken)).Id;
            string workspaceId = project.WorkspaceId;
            var startTime = new DateTimeOffset(DateTime.Today.AddHours(9));
            var timeEntry = new TimeEntryReq
            (
                project.Id,
                startTime,
                taskId: task?.Id,
                billable: project.Billable,
                end: startTime.AddMinutes(minutes),
                tagIds: (tagId != null) ? new List<string> {tagId} : null
            );

            await _clockifyService.AddTimeEntryAsync(clockifyToken, workspaceId, timeEntry);

            var todayEntries = await _clockifyService.GetHydratedTimeEntriesAsync(clockifyToken, workspaceId, userId,
                new DateTimeOffset(DateTime.Today), new DateTimeOffset(DateTime.Today.AddDays(1)));

            return todayEntries
                .Where(entry => entry.TimeInterval.Start.HasValue && entry.TimeInterval.End.HasValue)
                .Select(entry => (entry.TimeInterval.End!.Value - entry.TimeInterval.Start!.Value).TotalHours)
                .Sum();
        }
    }
}