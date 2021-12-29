using System;
using Bot.Remind;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Clockify
{
    public class EntryFillRemindService : SpecificRemindService
    {

        public EntryFillRemindService(IUserProfilesProvider userProfilesProvider, IConfiguration configuration,
            ICompositeNeedReminderService compositeNeedRemindService, IClockifyMessageSource messageSource,
            ILogger<EntryFillRemindService> logger) :
            base(userProfilesProvider, configuration, compositeNeedRemindService,
                messageSource, logger)
        {
        }
    }
}