using System.Threading.Tasks;
using Bot.States;

namespace Bot.Services.Reminds
{
    public interface INeedRemindService
    {
        public Task<bool> ReminderIsNeeded(UserProfile profile);
    }
}