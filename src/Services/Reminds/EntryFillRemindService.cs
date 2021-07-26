using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;

namespace Bot.Services.Reminds
{
    public class EntryFillRemindService : GenericRemindService, IEntryFillRemindService
    {
        private static async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            const string text = "Hey! I can see you have not filled up entirely your time sheet for the day. " +
                                "Remember, fill in earlier to make the entries really reflect what you did. " +
                                "I'll keep reminding you until you comply 🙃, if I become too annoying just ask me to stop.";

            await turnContext.SendActivityAsync(text, cancellationToken: cancellationToken);
        }

        public EntryFillRemindService(IUserProfilesProvider userProfilesProvider, IConfiguration configuration,
            INeedRemindService needRemindService) :
            base(userProfilesProvider, configuration, needRemindService, BotCallback)
        {
        }
    }
}