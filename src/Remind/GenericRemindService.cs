using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Remind
{
    public abstract class GenericRemindService : IRemindService
    {
        private readonly IUserProfilesProvider _userProfilesProvider;
        private readonly ICompositeNeedReminderService _compositeNeedRemindService;
        private readonly string _appId;
        private readonly BotCallbackHandler _botCallback;
        private readonly ILogger _logger;

        protected GenericRemindService(IUserProfilesProvider userProfilesProvider, IConfiguration configuration,
            ICompositeNeedReminderService compositeNeedReminderService, BotCallbackHandler botCallback,
            ILogger logger)
        {
            _userProfilesProvider = userProfilesProvider;
            _compositeNeedRemindService = compositeNeedReminderService;
            _botCallback = botCallback;
            _logger = logger;
            _appId = configuration["MicrosoftAppId"];
            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString();
            }
        }

        public async Task<string> SendReminderAsync(IBotFrameworkHttpAdapter adapter)
        {
            var reminderCounter = 0;

            async Task<SpecificRemindService.ReminderType> ReminderNeeded(UserProfile u) => await _compositeNeedRemindService.ReminderIsNeeded(u);

            List<UserProfile> userProfiles = await _userProfilesProvider.GetUserProfilesAsync();

            //Fetch all users where the ReminderType is not set to "NoReminder"
            List<UserProfile> userToRemind = userProfiles
                .Where(u => u.ClockifyTokenId != null && u.ConversationReference != null)
                .Where(u => ReminderNeeded(u).Result != SpecificRemindService.ReminderType.NoReminder)
                .ToList();

            foreach (var userProfile in userToRemind)
            {
                try
                {
                    ((BotAdapter)adapter).ContinueConversationAsync(
                        _appId,
                        userProfile!.ConversationReference,
                        _botCallback,
                        default).Wait(1000);
                    reminderCounter++;
                }
                catch (Exception e)
                {
                    // Just logging the exception is sufficient, we do not want to stop other reminders.
                    _logger.LogError(e, "Reminder not sent for user {UserId}", userProfile.UserId);
                }
            }

            return $"Sent reminder to {reminderCounter} users";
        }
    }
}