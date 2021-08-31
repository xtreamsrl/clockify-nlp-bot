using System;
using System.Linq;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Data;
using Bot.Remind;
using Bot.States;

namespace Bot.Clockify
{
    public class TimeSheetNotFullEnough : INeedRemindService
    {
        private readonly IClockifyService _clockifyService;
        private readonly ITokenRepository _tokenRepository;

        public TimeSheetNotFullEnough(IClockifyService clockifyService, ITokenRepository tokenRepository)
        {
            _clockifyService = clockifyService;
            _tokenRepository = tokenRepository;
        }

        public async Task<bool> ReminderIsNeeded(UserProfile userProfile)
        {
            try
            {
                var tokenData = await _tokenRepository.ReadAsync(userProfile.ClockifyTokenId!);
                string clockifyToken = tokenData.Value;
                string userId = userProfile.UserId ?? throw new ArgumentNullException(nameof(userProfile.UserId));
                var workspaces = await _clockifyService.GetWorkspacesAsync(clockifyToken);
                var start = new DateTimeOffset(DateTime.Today);
                var end = DateTimeOffset.Now;

                double totalHoursInserted = (await Task.WhenAll(workspaces.Select(ws => 
                        _clockifyService.GetHydratedTimeEntriesAsync(clockifyToken, ws.Id, userId, start, end))))
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