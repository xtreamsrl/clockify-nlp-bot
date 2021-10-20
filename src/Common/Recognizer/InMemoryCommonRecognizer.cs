using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace Bot.Common.Recognizer
{
    public class InMemoryCommonRecognizer : IRecognizer
    {
        public Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                return null;
            }

            string? utterance = turnContext.Activity?.AsMessageActivity()?.Text;

            return Task.FromResult(
                new RecognizerResult
                {
                    Text = utterance,
                    Intents = new Dictionary<string, IntentScore> { { string.Empty, new IntentScore { Score = 1.0 } } },
                    Entities = new JObject(),
                });
        }

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            // TODO here we need to use our model, otherwise is complex to reconstruct luis response based on the intent
            // if (turnContext.Activity.Type != ActivityTypes.Message)
            // {
            //     return null;
            // }

            string? utterance = turnContext.Activity?.AsMessageActivity()?.Text;
            TimeSurveyBotLuis luisResult = null;

            if (string.IsNullOrWhiteSpace(utterance))
            {
                luisResult = new TimeSurveyBotLuis()
                {
                    Text = utterance,
                    Intents = new Dictionary<TimeSurveyBotLuis.Intent, IntentScore>
                        { { TimeSurveyBotLuis.Intent.None, new IntentScore { Score = 1.0 } } },
                };
            }
            else
            {
                var (intents, entities) = ExtractIntentsAndEntities(utterance);
                luisResult = new TimeSurveyBotLuis
                {
                    Text = utterance,
                    Intents = intents,
                    // Entities = entities,
                };
            }

            // TODO Verify
            return (object) luisResult is T ? (T)(object) luisResult : default;
        }


        private static (Dictionary<TimeSurveyBotLuis.Intent, IntentScore> intents, JObject entities)
            ExtractIntentsAndEntities(string utterance)
        {
            int indexOfColon = utterance.IndexOf(":", StringComparison.Ordinal);
            
            string intentString = indexOfColon == -1 ? utterance : utterance[..indexOfColon];
            var intent = Enum.TryParse(intentString, true, out TimeSurveyBotLuis.Intent result)
                ? result
                : TimeSurveyBotLuis.Intent.None;
            var intents = new Dictionary<TimeSurveyBotLuis.Intent, IntentScore>
                { { intent, new IntentScore { Score = 1.0 } } };

            // string[] entitiesStrings = utterance[indexOfColon..].Split(",");
            JObject entities = new JObject();

            return (intents, entities);
        }
    }
}