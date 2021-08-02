using System;
using System.Linq;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.States;

namespace Bot.Services.Reminds
{
    public class TimeSheetNotFullEnough : INeedRemindService
    {
        private readonly IClockifyService _clockifyService;

        public TimeSheetNotFullEnough(IClockifyService clockifyService)
        {
            _clockifyService = clockifyService;
        }

        public async Task<bool> ReminderIsNeeded(UserProfile profile)
        {
            try
            {
                string token = profile.ClockifyToken ?? throw new ArgumentNullException(nameof(profile.ClockifyToken));
                string userId = profile.UserId ?? throw new ArgumentNullException(nameof(profile.UserId));
                var workspaces = await _clockifyService.GetWorkspacesAsync(profile.ClockifyToken);
                var start = new DateTimeOffset(DateTime.Today);
                var end = DateTimeOffset.Now;

                double totalHoursInserted = (await Task.WhenAll(workspaces.Select(ws => 
                        _clockifyService.GetHydratedTimeEntriesAsync(token, ws.Id, userId, start, end))))
                    .SelectMany(p => p)
                    .Sum(e =>
                    {
                        if (e.TimeInterval.End != null && e.TimeInterval.Start != null)
                        {
                            return (e.TimeInterval.End.Value - e.TimeInterval.Start.Value).TotalHours;
                        }
                        return 0;
                    });
                return totalHoursInserted < 6;
            }
            catch (Exception _)
            {
                return false;
            }
        }
    }
}