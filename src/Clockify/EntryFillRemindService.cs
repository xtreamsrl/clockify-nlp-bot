using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Remind;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;

namespace Bot.Clockify
{
    public class EntryFillRemindService : GenericRemindService
    {
        private static async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            const string text = "Hey! I can see you have not filled up entirely your time sheet for the day. " +
                                "Remember, fill in earlier to make the entries really reflect what you did. " +
                                "I'll keep reminding you until you comply 🙃, if I become too annoying just ask me to stop.";

            await turnContext.SendActivityAsync(text, cancellationToken: cancellationToken);
        }

        public EntryFillRemindService(IUserProfilesProvider userProfilesProvider, IConfiguration configuration,
            ICompositeNeedReminderService compositeNeedRemindService) :
            base(userProfilesProvider, configuration, compositeNeedRemindService, BotCallback)
        {
        }
    }
}