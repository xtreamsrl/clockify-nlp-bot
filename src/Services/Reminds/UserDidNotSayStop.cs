using System;
using System.Threading.Tasks;
using Bot.States;

namespace Bot.Services.Reminds
{
    public class UserDidNotSayStop: INeedRemindService
    {
        public Task<bool> ReminderIsNeeded(UserProfile profile)
        {
            return Task.FromResult(profile.StopRemind?.ToUniversalTime() != DateTime.Today.ToUniversalTime());
        }
    }
}