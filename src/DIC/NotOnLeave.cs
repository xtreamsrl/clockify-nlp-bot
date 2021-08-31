using System;
using System.Linq;
using System.Threading.Tasks;
using Bot.Data;
using Bot.Remind;
using Bot.States;

namespace Bot.DIC
{
    public class NotOnLeave : INeedRemindService
    {
        private readonly IDipendentiInCloudService _dipendentiInCloudService;
        private readonly ITokenRepository _tokenRepository;

        public NotOnLeave(IDipendentiInCloudService dipendentiInCloudService, ITokenRepository tokenRepository)
        {
            _dipendentiInCloudService = dipendentiInCloudService;
            _tokenRepository = tokenRepository;
        }

        public async Task<bool> ReminderIsNeeded(UserProfile userProfile)
        {
            if (userProfile.DicTokenId == null) return true;
            var tokenData = await _tokenRepository.ReadAsync(userProfile.DicTokenId!);
            string dicToken = tokenData.Value;
            
            var timesheet =
                await _dipendentiInCloudService.GetTimesheetForDay(DateTime.Today, dicToken,
                    userProfile.EmployeeId!.Value);
            int onLeaveHours = timesheet.reasons
                .Where(r => r.reason.id != 34)
                .Select(r => r.duration ?? 0)
                .Sum();
            return !timesheet.closed & onLeaveHours < 8 * 60;
        }
    }
}