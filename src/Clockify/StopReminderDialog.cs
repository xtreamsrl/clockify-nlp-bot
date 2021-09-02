using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Clockify
{
    public class StopReminderDialog : ComponentDialog
    {
        private readonly UserState _userState;
        private readonly IClockifyMessageSource _messageSource;

        private const string StopWaterfall = "StopWaterfall";

        public StopReminderDialog(UserState userState, IClockifyMessageSource messageSource)
        {
            _userState = userState;
            _messageSource = messageSource;
            AddDialog(new WaterfallDialog(StopWaterfall, new List<WaterfallStep>
            {
                FeedbackAndExitAsync
            }));
            Id = nameof(StopReminderDialog);
        }

        private async Task<DialogTurnResult> FeedbackAndExitAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var userProfile =
                await StaticUserProfileHelper.GetUserProfileAsync(_userState, stepContext.Context, cancellationToken);

            if (userProfile.StopRemind?.ToUniversalTime() == DateTime.Today.ToUniversalTime())
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(
                    _messageSource.RemindStoppedAlready), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            userProfile.StopRemind = DateTime.Today.ToUniversalTime();
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(_messageSource.RemindStopAnswer),
                cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}