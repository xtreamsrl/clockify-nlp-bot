using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.States;

namespace Bot.Remind
{
    public interface ICompositeNeedReminderService
    {
        Task<bool> ReminderIsNeeded(UserProfile profile);
    }
    
    public class CompositeNeedReminderService: ICompositeNeedReminderService
    {
        private readonly IEnumerable<INeedRemindService> _services;

        public CompositeNeedReminderService(IEnumerable<INeedRemindService> services)
        {
            _services = services;
        }

        public async Task<bool> ReminderIsNeeded(UserProfile profile)
        {
            bool[] conditions = await Task.WhenAll(_services.Select(service => service.ReminderIsNeeded(profile)));
            return conditions.All(c => c);
        }
    }
}