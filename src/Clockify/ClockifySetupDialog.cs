using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Clockify.Models;
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

        private readonly IClockifyService _clockifyService;
        private readonly UserState _userState;
        private readonly ITokenRepository _tokenRepository;
        private readonly IClockifyMessageSource _messageSource;

        public ClockifySetupDialog(UserState userState, IClockifyService clockifyService,
            ITokenRepository tokenRepository, IClockifyMessageSource messageSource) : base(nameof(ClockifySetupDialog))
        {
            _userState = userState;
            _clockifyService = clockifyService;
            _tokenRepository = tokenRepository;
            _messageSource = messageSource;

            AddDialog(new WaterfallDialog(TokenWaterfall, new List<WaterfallStep>
            {
                PromptForTokenAsync,
                FeedbackAndExitAsync
            }));
            AddDialog(new TextPrompt(AskForTokenStep, ClockifyTokenValidatorAsync));
            InitialDialogId = TokenWaterfall;
        }

        private async Task<DialogTurnResult> PromptForTokenAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(AskForTokenStep,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(_messageSource.SetupRequest),
                    RetryPrompt = MessageFactory.Text(_messageSource.SetupReject)
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> FeedbackAndExitAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(_messageSource.SetupFeedback), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<bool> ClockifyTokenValidatorAsync(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            string? token = promptContext.Recognized.Value;

            if (string.IsNullOrWhiteSpace(token) || token.Length > 100)
            {
                return false;
            }

            try
            {
                var userProfile = await _userState.CreateProperty<UserProfile>("UserProfile")
                    .GetAsync(promptContext.Context, () => new UserProfile(), cancellationToken);

                UserDo currentUser = await _clockifyService.GetCurrentUserAsync(token);
                var tokenData = await _tokenRepository.WriteAsync(token, userProfile.ClockifyTokenId);
                userProfile.ClockifyTokenId = tokenData.Id;
                userProfile.ClockifyToken = null;
                userProfile.UserId = currentUser.Id;
                if (currentUser.Name != null)
                {
                    userProfile.FirstName = currentUser.Name.Split(" ")[0]; //TODO: this might be wrong
                    userProfile.LastName = new string(currentUser.Name.Skip(userProfile.FirstName.Length + 1).ToArray());
                    userProfile.Email = currentUser.Email;
                }

                return true;
            }
            catch (ErrorResponseException)
            {
                return false;
            }
        }
    }
}