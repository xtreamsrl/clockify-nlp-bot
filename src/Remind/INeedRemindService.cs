using System.Threading.Tasks;
using Bot.States;

namespace Bot.Remind
{
    public interface INeedRemindService
    {
        public Task<bool> ReminderIsNeeded(UserProfile userProfile);
    }
}