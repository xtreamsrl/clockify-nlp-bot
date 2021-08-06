using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Data;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Bot.Clockify
{
    public class ClockifySetupDialog : ComponentDialog
    {
        private const string TokenWaterfall = "TokenWaterfall";
        private const string AskForTokenStep = "AskForToken";

        public const string Feedback = "Thanks! We will use this token whenever establishing a " +
                                       "Clockify connection from now on ⚙";

        public const string Request = "Please enter your Clockify API token. If you have no idea what that is, " +
                                      "don't worry, just look for it in [this page](https://clockify.me/user/settings)";

        public const string Reject = "Sorry, but this token is not valid. Please give me a proper one to continue";

        private readonly IClockifyService _clockifyService;
        private readonly UserState _userState;
        private readonly ITokenRepository _tokenRepository;

        public ClockifySetupDialog(UserState userState, IClockifyService clockifyService,
            ITokenRepository tokenRepository) : base(nameof(ClockifySetupDialog))
        {
            _userState = userState;
            _clockifyService = clockifyService;
            _tokenRepository = tokenRepository;

            AddDialog(new WaterfallDialog(TokenWaterfall, new List<WaterfallStep>
            {
                PromptForTokenAsync,
                FeedbackAndExitAsync
            }));
            AddDialog(new TextPrompt(AskForTokenStep, ClockifyTokenValidatorAsync));
            InitialDialogId = TokenWaterfall;
        }

        private static async Task<DialogTurnResult> PromptForTokenAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(AskForTokenStep,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(Request),
                    RetryPrompt = MessageFactory.Text(Reject)
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> FeedbackAndExitAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(Feedback), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<bool> ClockifyTokenValidatorAsync(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            string? token = promptContext.Recognized.Value;

            try
            {
                var userProfile = await _userState.CreateProperty<UserProfile>("UserProfile")
                    .GetAsync(promptContext.Context, () => new UserProfile(), cancellationToken);

                string? userId = _clockifyService.GetCurrentUserAsync(token).Result.Id;
                
                var tokenData = await _tokenRepository.WriteAsync(token, userProfile.ClockifyTokenId);
                userProfile.ClockifyToken = tokenData.Value;
                userProfile.ClockifyTokenId = tokenData.Id;
                
                userProfile.UserId = userId;
                return true;
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                {
                    if (e is ErrorResponseException)
                    {
                        return false;
                    }
                }

                // Unexpected exception occurred
                throw;
            }
        }
    }
}