using System;
using Bot.Remind;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Clockify
{
    public class EntryFillRemindService : GenericRemindService
    {
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

        public EntryFillRemindService(IUserProfilesProvider userProfilesProvider, IConfiguration configuration,
            ICompositeNeedReminderService compositeNeedRemindService, IClockifyMessageSource messageSource,
            ILogger<EntryFillRemindService> logger) :
            base(userProfilesProvider, configuration, compositeNeedRemindService,
                BotCallbackMaker(() => messageSource.RemindEntryFill), logger)
        {
        }
    }
}