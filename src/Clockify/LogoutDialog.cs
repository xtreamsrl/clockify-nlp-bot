using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Data;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Bot.Clockify
{
    public class LogoutDialog : ComponentDialog
    {
        private const string LogoutWaterfall = nameof(LogoutWaterfall);
        private readonly UserState _userState;
        private readonly IClockifyMessageSource _messageSource;
        private readonly ITokenRepository _tokenRepository;

        private const string Yes = "yes";
        private const string No = "no";

        public LogoutDialog(UserState userState, IClockifyMessageSource messageSource, ITokenRepository tokenRepository)
        {
            _userState = userState;
            _messageSource = messageSource;
            _tokenRepository = tokenRepository;
            AddDialog(new WaterfallDialog(LogoutWaterfall, new List<WaterfallStep>
            {
                ConfirmationStep,
                LogoutStep
            }));
            AddDialog(new TextPrompt(nameof(ConfirmationStep), LogoutValidator));
            Id = nameof(LogoutDialog);
        }

        private async Task<DialogTurnResult> ConfirmationStep(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var suggestions = new List<CardAction>
            {
                new CardAction
                {
                    Title = Yes, Type = ActionTypes.MessageBack, Text = Yes, Value = Yes,
                    DisplayText = Yes
                },
                new CardAction
                {
                    Title = No, Type = ActionTypes.MessageBack, Text = No, Value = No,
                    DisplayText = No
                }
            };
            var activity = MessageFactory.Text(_messageSource.LogoutPrompt);
            activity.SuggestedActions = new SuggestedActions { Actions = suggestions };
            return await stepContext.PromptAsync(nameof(ConfirmationStep), new PromptOptions
            {
                Prompt = activity,
                RetryPrompt = MessageFactory.Text(_messageSource.LogoutRetryPrompt),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> LogoutStep(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var result = stepContext.Result.ToString();
            switch (result?.ToLower())
            {
                case Yes:
                    var userProfile =
                        await StaticUserProfileHelper.GetUserProfileAsync(_userState, stepContext.Context,
                            cancellationToken);
                    
                    //Removes the token from the repository! This change reflects immediateley also within all caches 
                    //and also on the remote key vault!
                    await _tokenRepository.RemoveAsync(userProfile.ClockifyTokenId!);
                    
                    //Now we can also remove the tokenID from the UserProfile
                    userProfile.ClockifyTokenId = null;
                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text(_messageSource.LogoutYes), cancellationToken);
                    break;
                case No:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(_messageSource.LogoutNo),
                        cancellationToken);
                    break;
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static Task<bool> LogoutValidator(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            string? pValue = promptContext.Recognized.Value;
            return Task.FromResult(!string.IsNullOrEmpty(pValue) &&
                                   (pValue.ToLower() == Yes || pValue.ToLower() == No));
        }
    }
}