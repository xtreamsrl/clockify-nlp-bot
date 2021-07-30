using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Exceptions;
using Bot.Recognizers;
using Bot.Utils;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Tasks;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Dialogs
{
    public class StopReminderDialog : ComponentDialog
    {
        private readonly UserState _userState;


        private const string StopWaterfall = "StopWaterfall";

        public StopReminderDialog(UserState userState)
        {
            _userState = userState;
            AddDialog(new WaterfallDialog(StopWaterfall, new List<WaterfallStep>
            {
                FeedbackAndExitAsync
            }));
            Id = DialogIdProvider.GetDialogId(typeof(StopReminderDialog));
        }
        
        private async Task<DialogTurnResult> FeedbackAndExitAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var userProfile =
                await StaticUserProfileHelper.GetUserProfileAsync(_userState, stepContext.Context, cancellationToken);
            
            if (userProfile.StopRemind?.ToUniversalTime() == DateTime.Today.ToUniversalTime())
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(
                    "You already told me to stop sending you reminds for the day, there's no need to insist"), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            userProfile.StopRemind = DateTime.Today.ToUniversalTime();
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(
                "Ok, no more reminders for today, you have my word 🤙"), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}