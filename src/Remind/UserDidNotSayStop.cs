using System;
using System.Threading.Tasks;
using Bot.States;

namespace Bot.Remind
{
    public class UserDidNotSayStop: INeedRemindService
    {
        public Task<bool> ReminderIsNeeded(UserProfile userProfile)
        {
            return Task.FromResult(userProfile.StopRemind?.ToUniversalTime() != DateTime.Today.ToUniversalTime());
        }
    }
}