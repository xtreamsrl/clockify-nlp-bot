using System.Threading;
using System.Threading.Tasks;
using Bot.Clockify;
using Bot.Remind;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.DIC
{
    public class SmartWorkingRemindService : GenericRemindService
    {
        private static BotCallbackHandler BotCallback(ClockifySetupDialog dialog, ConversationState conversationState)
        {
            return (turn, token) => BotCallback(dialog, conversationState, turn, token);
        }

        private static async Task BotCallback(Dialog dialog, ConversationState conversationState,
            ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await dialog.RunAsync(turnContext,
                conversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
            await conversationState.SaveChangesAsync(turnContext, true, cancellationToken);
        }

        public SmartWorkingRemindService(IUserProfilesProvider userProfilesProvider, IConfiguration configuration,
            ICompositeNeedReminderService compositeNeedReminderService, ClockifySetupDialog clockifySetup,
            ConversationState conversationState, ILogger<SmartWorkingRemindService> logger) :
            base(userProfilesProvider, configuration, compositeNeedReminderService,
                BotCallback(clockifySetup, conversationState), logger)
        {
        }
    }
}