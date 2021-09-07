using Bot.Common;
using Bot.Remind;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;

namespace Bot.Clockify
{
    public class EntryFillRemindService : GenericRemindService
    {
        private static BotCallbackHandler BotCallbackMaker(string text)
        {
            return async (turn, token) => await turn.SendActivityAsync(text, cancellationToken: token);
        }

        public EntryFillRemindService(IUserProfilesProvider userProfilesProvider, IConfiguration configuration,
            ICompositeNeedReminderService compositeNeedRemindService, IClockifyMessageSource messageSource) :
            base(userProfilesProvider, configuration, compositeNeedRemindService,
                BotCallbackMaker(messageSource.RemindEntryFill))
        {
        }
    }
}