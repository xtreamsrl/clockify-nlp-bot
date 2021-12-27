using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Clockify;
using Bot.DIC;
using Bot.States;

namespace Bot.Remind
{
    public interface ICompositeNeedReminderService
    {
        Task<SpecificRemindService.ReminderType> ReminderIsNeeded(UserProfile profile);
    }
    
    public class CompositeNeedReminderService: ICompositeNeedReminderService
    {
        private readonly IEnumerable<INeedRemindService> _services;

        public CompositeNeedReminderService(IEnumerable<INeedRemindService> services)
        {
            _services = services;
        }

        public async Task<SpecificRemindService.ReminderType> ReminderIsNeeded(UserProfile profile)
        {
            var reminder = SpecificRemindService.ReminderType.NoReminder;
            
            //Check every reminder within all services
            foreach (var service in _services)
            {
                var serviceType = typeof(PastDayNotComplete);
                var reminderIsNeeded = await service.ReminderIsNeeded(profile);

                //Check if the particular reminder was set to true
                if (reminderIsNeeded)
                {
                    //The reminder for this service is needed, check why it is needed and set the flags
                    if (service.GetType() == typeof(PastDayNotComplete)) reminder |= SpecificRemindService.ReminderType.YesterdayReminder;
                    if (service.GetType() == typeof(TimeSheetNotFullEnough)) reminder |= SpecificRemindService.ReminderType.TodayReminder;
                }
                else
                {
                    //The reminder for this service is not needed. Therefore we check, what was negative and set the appropriate flag!
                    if (service.GetType() == typeof(EndOfWorkingDay)) reminder |= SpecificRemindService.ReminderType.OutOfWorkTime;
                    if (service.GetType() == typeof(UserDidNotSayStop)) reminder |= SpecificRemindService.ReminderType.UserSaidStop;
                    if (service.GetType() == typeof(NotOnLeave)) reminder |= SpecificRemindService.ReminderType.UserOnLeave;
                }
            }
            return reminder;
        }
    }
}