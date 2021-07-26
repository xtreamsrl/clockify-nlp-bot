using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;

namespace Bot.Services.Reminds
{
    public class GenericRemindService : IRemindService
    {
        private readonly IUserProfilesProvider _userProfilesProvider;
        private readonly INeedRemindService _needRemindService;
        private readonly string _appId;
        private readonly BotCallbackHandler _botCallback;

        protected GenericRemindService(IUserProfilesProvider userProfilesProvider, IConfiguration configuration,
            INeedRemindService needRemindService, BotCallbackHandler botCallback)
        {
            _userProfilesProvider = userProfilesProvider;
            _needRemindService = needRemindService;
            _botCallback = botCallback;
            _appId = configuration["MicrosoftAppId"];
            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString();
            }
        }

        public async Task<string> SendReminderAsync(IBotFrameworkHttpAdapter adapter)
        {
            var reminderCounter = 0;

            async Task<bool> ReminderNeeded(UserProfile u) => await _needRemindService.ReminderIsNeeded(u);

            List<UserProfile> userProfiles = await _userProfilesProvider.GetUserProfilesAsync();

            List<UserProfile> userToRemind = userProfiles
                .Where(u => u.ClockifyToken != null && u.ConversationReference != null)
                .Where(u => ReminderNeeded(u).Result)
                .ToList();

            foreach (var userProfile in userToRemind)
            {
                try
                {
                    ((BotAdapter) adapter).ContinueConversationAsync(
                        _appId,
                        userProfile!.ConversationReference,
                        _botCallback,
                        default).Wait(1000);
                    reminderCounter++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return $"Sent reminder to {reminderCounter} users";
        }
    }
}