using System.Linq;
using System.Threading.Tasks;
using Bot.States;

namespace Bot.Services.Reminds
{
    public class CompositeNeedReminderService: INeedRemindService
    {
        private readonly INeedRemindService[] _services;

        public CompositeNeedReminderService(params INeedRemindService[] services)
        {
            _services = services;
        }

        public async Task<bool> ReminderIsNeeded(UserProfile profile)
        {
            var conditions = await Task.WhenAll(_services.Select(service => service.ReminderIsNeeded(profile)));
            return conditions.All(c => c);
        }
    }
}