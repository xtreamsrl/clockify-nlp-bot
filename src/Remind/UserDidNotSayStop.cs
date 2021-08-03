using System;
using System.Threading.Tasks;
using Bot.States;

namespace Bot.Remind
{
    public class UserDidNotSayStop: INeedRemindService
    {
        public Task<bool> ReminderIsNeeded(UserProfile profile)
        {
            return Task.FromResult(profile.StopRemind?.ToUniversalTime() != DateTime.Today.ToUniversalTime());
        }
    }
}