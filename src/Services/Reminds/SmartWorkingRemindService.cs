using System.Threading;
using System.Threading.Tasks;
using Bot.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;

namespace Bot.Services.Reminds
{
    public class SmartWorkingRemindService : GenericRemindService, ISmartWorkingRemindService
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
            INeedRemindService needRemindService, ClockifySetupDialog clockifySetup, ConversationState conversationState) :
            base(userProfilesProvider, configuration, needRemindService, BotCallback(clockifySetup, conversationState))
        {
        }
    }
}