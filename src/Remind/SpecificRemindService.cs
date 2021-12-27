using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Clockify;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Remind
{
    public abstract class SpecificRemindService : ISpecificRemindService
    {
        private readonly IUserProfilesProvider _userProfilesProvider;
        private readonly IClockifyMessageSource _messageSource;
        private readonly ICompositeNeedReminderService _compositeNeedRemindService;
        private readonly string _appId;
        private readonly ILogger _logger;

        private static BotCallbackHandler BotCallbackMaker(Func<string> getResource)
        {
            return async (turn, token) =>
            {
                string text = getResource();
                if (Uri.IsWellFormedUriString(text, UriKind.RelativeOrAbsolute))
                {
                    // TODO: support other content types
                    await turn.SendActivityAsync(MessageFactory.Attachment(new Attachment("image/png", text)), token);
                }
                else
                {
                    await turn.SendActivityAsync(MessageFactory.Text(text), token);
                }
            };
        }

        protected SpecificRemindService(IUserProfilesProvider userProfilesProvider, IConfiguration configuration,
            ICompositeNeedReminderService compositeNeedReminderService, IClockifyMessageSource messageSource,
            ILogger logger)
        {
            _userProfilesProvider = userProfilesProvider;
            _compositeNeedRemindService = compositeNeedReminderService;
            _messageSource = messageSource;
            _logger = logger;
            _appId = configuration["MicrosoftAppId"];
            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString();
            }
        }

        [Flags]
        public enum ReminderType
        {
            NoReminder = 0,
            TodayReminder = 1,
            YesterdayReminder = 2,
            WeekReminder = 4,
            OutOfWorkTime = 8,
            UserSaidStop = 16,
            UserOnLeave = 32
        };


        private bool SendSpecificReminderType(IBotFrameworkHttpAdapter adapter, UserProfile userProfile,
            ReminderType reminderType)
        {
            var callback = BotCallbackMaker(() => _messageSource.RemindEntryFill);
            switch (reminderType)
            {
                case ReminderType.TodayReminder:
                    callback = BotCallbackMaker(() => _messageSource.RemindEntryFill);
                    break;

                case ReminderType.YesterdayReminder:
                    callback = BotCallbackMaker(() => _messageSource.RemindEntryFillYesterday);
                    break;
            }

            try
            {
                //TODO Change _botCallback according to the reminder type
                ((BotAdapter)adapter).ContinueConversationAsync(
                    _appId,
                    userProfile!.ConversationReference,
                    callback,
                    default).Wait(1000);
            }
            catch (Exception e)
            {
                // Just logging the exception is sufficient, we do not want to stop other reminders.
                _logger.LogError(e, "Reminder not sent for user {UserId}", userProfile.UserId);
                return false;
            }

            return true;
        }

        public async Task<string> SendReminderAsync(IBotFrameworkHttpAdapter adapter, ReminderType typesToRemind)
        {
            var reminderCounter = 0;
            var userCounter = 0;
            //Check, whether we need to remind at least one event
            if (typesToRemind != ReminderType.NoReminder)
            {
                async Task<ReminderType> ReminderNeeded(UserProfile u) =>
                    await _compositeNeedRemindService.ReminderIsNeeded(u);

                List<UserProfile> userProfiles = await _userProfilesProvider.GetUserProfilesAsync();

                //Search for all users where a reminder was set to something else than "NoReminder"
                List<UserProfile> validUsers = userProfiles
                    .Where(u => u.ClockifyTokenId != null && u.ConversationReference != null)
                    .ToList();

                foreach (var userProfile in validUsers)
                {
                    var userReminderTypes = ReminderNeeded(userProfile).Result;

                    //Check if we need to remind the user
                    if (userReminderTypes != ReminderType.NoReminder)
                    {
                        userCounter++;
                        
                        //Check, if the user needs a reminder for today and if we also have requested a reminder for today.
                        if (userReminderTypes.HasFlag(ReminderType.TodayReminder) &&
                            typesToRemind.HasFlag(ReminderType.TodayReminder))
                        {
                            if (SendSpecificReminderType(adapter, userProfile, ReminderType.TodayReminder))
                                reminderCounter++;
                        }

                        //Check, if the user needs a reminder for yesterday and if we also have requested a reminder for yesterday.
                        if (userReminderTypes.HasFlag(ReminderType.YesterdayReminder) &&
                            typesToRemind.HasFlag(ReminderType.YesterdayReminder))
                        {
                            if (SendSpecificReminderType(adapter, userProfile, ReminderType.YesterdayReminder))
                                reminderCounter++;
                        }
                    }
                }
            }

            return $"Sent {reminderCounter} reminder to {userCounter} users";
        }
    }
}