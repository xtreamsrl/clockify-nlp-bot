using System;
using System.Collections.Generic;
using Bot.Remind;
using Bot.States;
using Microsoft.AspNetCore.StaticFiles;
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
                string content = getResource();
                if (Uri.IsWellFormedUriString(content, UriKind.RelativeOrAbsolute))
                {
                    new FileExtensionContentTypeProvider().TryGetContentType(content, out string contentType);
                    var attachment = new Attachment(contentType, content);   
                    await turn.SendActivityAsync(new Activity(text: "", 
                        attachments:new List<Attachment> {attachment}), token);
                }
                else
                {
                    await turn.SendActivityAsync(MessageFactory.Text(content), token);
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