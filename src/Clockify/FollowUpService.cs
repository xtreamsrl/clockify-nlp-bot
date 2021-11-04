using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Common;
using Bot.States;
using FluentDateTime;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Clockify
{
    public class FollowUpService : IFollowUpService
    {
        private readonly IUserProfilesProvider _userProfilesProvider;
        private readonly string _microsoftAppId;
        private readonly IClockifyMessageSource _messageSource;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<FollowUpService> _logger;

        public FollowUpService(IUserProfilesProvider userProfilesProvider, IConfiguration configuration,
            IClockifyMessageSource messageSource, IDateTimeProvider dateTimeProvider, ILogger<FollowUpService> logger)
        {
            _userProfilesProvider = userProfilesProvider;
            _microsoftAppId = configuration["MicrosoftAppId"];
            if (string.IsNullOrEmpty(_microsoftAppId))
            {
                _microsoftAppId = Guid.NewGuid().ToString();
            }
            _logger = logger;
            _messageSource = messageSource;
            _dateTimeProvider = dateTimeProvider;
        }

        private static BotCallbackHandler FollowUpCallback(Func<string> getFollowUpMessage)
        {
            return async (turn, token) =>
            {
                string followUpMessage = getFollowUpMessage();
                await turn.SendActivityAsync(MessageFactory.Text(followUpMessage), token);
            };
        }

        public async Task<string> SendFollowUpAsync(BotAdapter adapter)
        {
            var botCallback = FollowUpCallback(() => _messageSource.FollowUp);
            var followUpCounter = 0;

            List<UserProfile> userProfiles = await _userProfilesProvider.GetUserProfilesAsync();

            var utcNow = _dateTimeProvider.DateTimeUtcNow();
            List<UserProfile> usersToFollowUp = userProfiles
                .Where(u => u.ClockifyTokenId == null && u.ConversationReference != null)
                .Where(u => u.LastFollowUpTimestamp is null)
                .Where(u => u.LastConversationUpdate >= utcNow.FirstDayOfYear().Date &&
                            u.LastConversationUpdate <= utcNow.Subtract(TimeSpan.FromDays(2)))
                .ToList();

            foreach (var userProfile in usersToFollowUp)
            {
                try
                {
                    adapter.ContinueConversationAsync(
                        _microsoftAppId,
                        userProfile.ConversationReference,
                        botCallback,
                        default).Wait(1000);
                    userProfile.LastFollowUpTimestamp = _dateTimeProvider.DateTimeUtcNow();
                    followUpCounter++;
                }
                catch (Exception e)
                {
                    // Just logging the exception is sufficient, we do not want to stop other reminders.
                    _logger.LogError(e, "Follow up not sent for user {UserId}", userProfile.UserId);
                }
            }

            return $"Sent follow up to {followUpCounter} users";
        }
    }
}