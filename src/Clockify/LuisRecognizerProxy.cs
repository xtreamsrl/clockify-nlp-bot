using System;
using System.Threading;
using System.Threading.Tasks;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using LuisPredictionOptions = Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions;

namespace Bot.Clockify
{
    // Proxy around LuisRecognizer
    public sealed class LuisRecognizerProxy : IRecognizer
    {
        private readonly LuisRecognizer? _recognizer;

        public LuisRecognizerProxy(IConfiguration configuration)
        {
            bool luisIsConfigured = !string.IsNullOrEmpty(configuration["LuisAppId"]) &&
                                    !string.IsNullOrEmpty(configuration["LuisAPIKey"]) &&
                                    !string.IsNullOrEmpty(configuration["LuisAPIHostName"]);
            if (!luisIsConfigured)
            {
                return;
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

        public bool IsConfigured => _recognizer != null;

        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            if (_recognizer == null) throw new ArgumentNullException();

            return await _recognizer.RecognizeAsync(turnContext, cancellationToken);
        }

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            if (_recognizer == null) throw new ArgumentNullException();

            return await _recognizer.RecognizeAsync<T>(turnContext, cancellationToken);
        }

        public async Task<(TimeSurveyBotLuis.Intent topIntent, TimeSurveyBotLuis._Entities._Instance entities)>
            RecognizeAsyncIntent(ITurnContext turnContext, CancellationToken cancellationToken, double minScore = 0.75)
        {
            var luisResult = await RecognizeAsync<TimeSurveyBotLuis>(turnContext, cancellationToken);
            var (topIntent, score) = luisResult.TopIntent();
            var entities = luisResult.Entities._instance;

            return score < minScore ? (TimeSurveyBotLuis.Intent.None, entities) : (topIntent, entities);
        }
    }
}