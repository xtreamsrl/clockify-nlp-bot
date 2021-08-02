using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Services.Reminds;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Bot.Clockify
{
    public class NotifyUsersDialog : ComponentDialog
    {
        private const string TaskWaterfall = "NotificationWaterfall";
        private const string AskForNotification = "AskForNotification";
        private const string NotificationSent = "Notification sent to {0} users";
        private readonly IUserProfilesProvider _userProfilesProvider;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;


        public NotifyUsersDialog(IUserProfilesProvider userProfilesProvider, IBotFrameworkHttpAdapter adapter, 
            IConfiguration configuration)
        {
            _userProfilesProvider = userProfilesProvider;
            _adapter = adapter;
            AddDialog(new WaterfallDialog(TaskWaterfall, new List<WaterfallStep>
            {
                PromptForNotificationAsync,
                FeedbackAndExitAsync
            }));
            Id = nameof(NotifyUsersDialog);
            _appId = configuration["MicrosoftAppId"];
            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString();
            }

            AddDialog(new TextPrompt(AskForNotification));
        }

        private async Task<DialogTurnResult> FeedbackAndExitAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var reminderCounter = 0;
            var notification = stepContext.Context.Activity;
            var users = await _userProfilesProvider.GetUserProfilesAsync();
            foreach (var userProfile in users)
            {
                try
                {
                    var notificationToSend = MessageFactory.Text(notification.Text);
                    notificationToSend.Attachments = notification.Attachments;
                    ((BotAdapter) _adapter).ContinueConversationAsync(
                        _appId,
                        userProfile!.ConversationReference,
                        SendAcivity(notificationToSend),
                        default).Wait(1000);
                    reminderCounter++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text(string.Format(NotificationSent, reminderCounter)), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private BotCallbackHandler SendAcivity(IActivity notification)
        {
            return (turn, token) => turn.SendActivityAsync(notification, token);
        }

        private static async Task<DialogTurnResult> PromptForNotificationAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(AskForNotification, new PromptOptions
            {
                Prompt = MessageFactory.Text("What's the notification you want to send?"),
            }, cancellationToken);
        }
    }
}