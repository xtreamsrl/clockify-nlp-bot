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
    public class TimeSheetNotFullEnough : INeedRemindService
    {
        private readonly IClockifyService _clockifyService;
        private readonly ITokenRepository _tokenRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        //Get de default hours to work. If not defined, assume 8hours
        public static readonly string DefaultWorkingHours =
            Environment.GetEnvironmentVariable("DEFAULT_WORKING_HOURS") ?? "8";

        //Get the minimum percentage of hours filled. If not defined, assume 75% of a default work day to be reported.
        //This leads to 6 hours
        public static readonly string MinimumHoursFilledPercentage =
            Environment.GetEnvironmentVariable("MINIMUM_HOURS_FILLED_PERCENTAGE") ?? "75";

        public TimeSheetNotFullEnough(IClockifyService clockifyService, ITokenRepository tokenRepository,
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
                var userStartToday = userNow.Date;
                var userEndOfToday = userStartToday.AddDays(1);

                double totalHoursInserted = (await Task.WhenAll(workspaces.Select(ws =>
                        _clockifyService.GetHydratedTimeEntriesAsync(clockifyToken, ws.Id, userId, userStartToday,
                            userEndOfToday))))
                    .SelectMany(p => p)
                    .Sum(e =>
                    {
                        if (e.TimeInterval.End != null && e.TimeInterval.Start != null)
                        {
                            return (e.TimeInterval.End.Value - e.TimeInterval.Start.Value).TotalHours;
                        }

                        return 0;
                    });

                //Check if we have defined the working hours on user level. If so, calculate the minimum.
                if (userProfile.WorkingHours != null)
                    return totalHoursInserted <
                           (userProfile.WorkingHours * (double.Parse(MinimumHoursFilledPercentage) / 100));
                
                //Calculate the minimum amount of hours to be reported based on the defaults.
                return totalHoursInserted < (double.Parse(DefaultWorkingHours) *
                                             (double.Parse(MinimumHoursFilledPercentage) / 100));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}