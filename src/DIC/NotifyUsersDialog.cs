using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Bot.DIC
{
    public class NotifyUsersDialog : ComponentDialog
    {
        private const string TaskWaterfall = "NotificationWaterfall";
        private const string AskForNotification = "AskForNotification";
        private readonly IUserProfilesProvider _userProfilesProvider;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly IDicMessageSource _messageSource;

        public NotifyUsersDialog(IUserProfilesProvider userProfilesProvider, IBotFrameworkHttpAdapter adapter, 
            IConfiguration configuration, IDicMessageSource messageSource)
        {
            _userProfilesProvider = userProfilesProvider;
            _adapter = adapter;
            _messageSource = messageSource;
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
                        SendActivity(notificationToSend),
                        default).Wait(1000);
                    reminderCounter++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text(string.Format(_messageSource.NotificationSent, reminderCounter)), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static BotCallbackHandler SendActivity(IActivity notification)
        {
            return (turn, token) => turn.SendActivityAsync(notification, token);
        }

        private async Task<DialogTurnResult> PromptForNotificationAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(AskForNotification, new PromptOptions
            {
                Prompt = MessageFactory.Text(_messageSource.NotificationQuestion),
            }, cancellationToken);
        }
    }
}