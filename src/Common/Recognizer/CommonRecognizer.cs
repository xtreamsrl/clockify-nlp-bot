using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using LuisPredictionOptions = Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions;

namespace Bot.Common.Recognizer
{
    public sealed class CommonRecognizer : IRecognizer
    {
        private readonly LuisRecognizer _recognizer;

        public CommonRecognizer(IConfiguration configuration)
        {
            bool luisIsConfigured = !string.IsNullOrEmpty(configuration["LuisAppId"]) &&
                                    !string.IsNullOrEmpty(configuration["LuisAPIKey"]) &&
                                    !string.IsNullOrEmpty(configuration["LuisAPIHostName"]);
            if (!luisIsConfigured)
            {
                throw new Exception(
                    "LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.");
            }

            var luisApplication = new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisAPIKey"],
                "https://" + configuration["LuisAPIHostName"]
            );

            // Set the recognizer options depending on which endpoint version you want to use.
            // More details can be found in https://docs.microsoft.com/en-gb/azure/cognitive-services/luis/luis-migration-api-v3
            var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication)
            {
                PredictionOptions = new LuisPredictionOptions
                {
                    IncludeInstanceData = true
                }
            };

            _recognizer = new LuisRecognizer(recognizerOptions);
        }

        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext,
            CancellationToken cancellationToken)
            => await _recognizer.RecognizeAsync(turnContext, cancellationToken);

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
            => await _recognizer.RecognizeAsync<T>(turnContext, cancellationToken);
        
    }
}