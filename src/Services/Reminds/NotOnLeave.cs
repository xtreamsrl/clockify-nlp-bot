using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.States;
using Microsoft.Azure.Cosmos.Linq;

namespace Bot.Services.Reminds
{
    public class NotOnLeave : INeedRemindService
    {
        private readonly DipendentiInCloudService _dipendentiInCloudService;

        public NotOnLeave(DipendentiInCloudService dipendentiInCloudService)
        {
            _dipendentiInCloudService = dipendentiInCloudService;
        }

        public async Task<bool> ReminderIsNeeded(UserProfile profile)
        {
            if (profile.DicToken == null) return true;
            var timesheet =
                await _dipendentiInCloudService.GetTimesheetForDay(DateTime.Today, profile.DicToken!,
                    profile.EmployeeId!.Value);
            int onLeaveHours = timesheet.reasons
                .Where(r => r.reason.id != 34)
                .Select(r => r.duration ?? 0)
                .Sum();
            return !timesheet.closed & onLeaveHours < 8 * 60;
        }
    }
}