using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Clockify;
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
                    if (service.GetType() == typeof(PastDayNotComplete) ||
                        service.GetType() == typeof(TimeSheetNotFullEnough))
                    {
                        if (service.GetType() == typeof(PastDayNotComplete)) reminder |= SpecificRemindService.ReminderType.YesterdayReminder;
                        if (service.GetType() == typeof(TimeSheetNotFullEnough)) reminder |= SpecificRemindService.ReminderType.TodayReminder;
                    }
                }
                else
                {
                    //As soon as one reminder check was negative, we return "NoReminder" since all checks needs to be true!
                    //TODO not always break! We can remind for yesterdays times even if it is early morning!
                    return SpecificRemindService.ReminderType.NoReminder;
                }
            }
            
            return reminder;

            // foreach (var service in _services.Select(service => service.ReminderIsNeeded(profile)))
            // {
            //     
            // }
            //
            // bool[] conditions = await Task.WhenAll(_services.Select(service => service.ReminderIsNeeded(profile)));
            // return conditions.All(c => c);
        }
    }
}