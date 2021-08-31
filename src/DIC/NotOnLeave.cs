using System;
using System.Linq;
using System.Threading.Tasks;
using Bot.Remind;
using Bot.States;

namespace Bot.DIC
{
    public class NotOnLeave : INeedRemindService
    {
        private readonly IDipendentiInCloudService _dipendentiInCloudService;

        public NotOnLeave(IDipendentiInCloudService dipendentiInCloudService)
        {
            _dipendentiInCloudService = dipendentiInCloudService;
        }

        public async Task<bool> ReminderIsNeeded(UserProfile userProfile)
        {
            if (userProfile.DicToken == null) return true;
            var timesheet =
                await _dipendentiInCloudService.GetTimesheetForDay(DateTime.Today, userProfile.DicToken!,
                    userProfile.EmployeeId!.Value);
            int onLeaveHours = timesheet.reasons
                .Where(r => r.reason.id != 34)
                .Select(r => r.duration ?? 0)
                .Sum();
            return !timesheet.closed & onLeaveHours < 8 * 60;
        }
    }
}