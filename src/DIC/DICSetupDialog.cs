using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Data;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Bot.DIC
{
    public class DicSetupDialog : ComponentDialog
    {
        private const string TokenWaterfall = "DICTokenWaterfall";
        private const string AskForTokenStep = "AskForDICToken";
        private const string Feedback = "Hi {0}, happy to meet you!";

        private const string Request = "Please enter your Dipendenti in Cloud authentication token. You can find it here";

        private const string Reject = "Sorry, but this token is not valid. Please give me a proper one to continue";

        private readonly IDipendentiInCloudService _dicService;
        private readonly UserState _userState;
        private readonly ITokenRepository _tokenRepository;

        public DicSetupDialog(UserState userState, IDipendentiInCloudService clockifyService,
            ITokenRepository tokenRepository) : base(nameof(DicSetupDialog))
        {
            _userState = userState;
            _dicService = clockifyService;
            _tokenRepository = tokenRepository;

            AddDialog(new WaterfallDialog(TokenWaterfall, new List<WaterfallStep>
            {
                PromptForTokenAsync,
                FeedbackAndExitAsync
            }));
            AddDialog(new TextPrompt(AskForTokenStep, DICTokenValidatorAsync));
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
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<bool> DICTokenValidatorAsync(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            string? token = promptContext.Recognized.Value;
            
            if (string.IsNullOrWhiteSpace(token) || token.Length > 200)
            {
                return false;
            }
            
            try
            {
                var userProfile = await _userState.CreateProperty<UserProfile>("UserProfile")
                    .GetAsync(promptContext.Context, () => new UserProfile(), cancellationToken);
                var employee = _dicService.GetCurrentEmployeeAsync(token).Result;
                
                var tokenData = await _tokenRepository.WriteAsync(token, userProfile.DicTokenId);
                userProfile.DicTokenId = tokenData.Id;
                userProfile.DicToken = null;
                
                userProfile.EmployeeId = employee.id;
                userProfile.FirstName = employee.first_name;
                userProfile.LastName = employee.last_name;
                Activity reply = MessageFactory.Text(string.Format(Feedback, userProfile.FirstName));
                await promptContext.Context.SendActivityAsync(reply, cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}