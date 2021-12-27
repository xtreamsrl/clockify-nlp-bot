using System;
using System.Linq;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Common;
using Bot.Data;
using Bot.Remind;
using Bot.States;

namespace Bot.Clockify
{
    public class PastDayNotComplete : INeedRemindService
    {
        private readonly IClockifyService _clockifyService;
        private readonly ITokenRepository _tokenRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public PastDayNotComplete(IClockifyService clockifyService, ITokenRepository tokenRepository,
            IDateTimeProvider dateTimeProvider)
        {
            _clockifyService = clockifyService;
            _tokenRepository = tokenRepository;
            _dateTimeProvider = dateTimeProvider;
        }
        
        public async Task<bool> ReminderIsNeeded(UserProfile userProfile)
        {
            try
            {
                var tokenData = await _tokenRepository.ReadAsync(userProfile.ClockifyTokenId!);
                string clockifyToken = tokenData.Value;
                string userId = userProfile.UserId ?? throw new ArgumentNullException(nameof(userProfile.UserId));
                var workspaces = await _clockifyService.GetWorkspacesAsync(clockifyToken);

                TimeZoneInfo userTimeZone = userProfile.TimeZone;
                var userNow = TimeZoneInfo.ConvertTime(_dateTimeProvider.DateTimeUtcNow(), userTimeZone);

                var userStartDay = userNow.Date.AddDays(-1); //Get past day

                //Check for weekends. If we got one, go back in time. 
                while (userStartDay.DayOfWeek == DayOfWeek.Sunday || userStartDay.DayOfWeek == DayOfWeek.Saturday)
                {
                    userStartDay = userStartDay.AddDays(-1); //Go back in time till we have no weekend anymore
                }
                var userEndDay = userStartDay.AddDays(1); //Add one day to the startDay for a 1 day range

                double totalHoursInserted = (await Task.WhenAll(workspaces.Select(ws =>
                        _clockifyService.GetHydratedTimeEntriesAsync(clockifyToken, ws.Id, userId, userStartDay,
                            userEndDay))))
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
            catch (Exception)
            {
                return false;
            }
        }
    }
}