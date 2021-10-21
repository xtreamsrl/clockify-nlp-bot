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
            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                return ConvertToT<T>(DefaultIntent());
            }

            string? utterance = turnContext.Activity?.AsMessageActivity()?.Text;
            TimeSurveyBotLuis luisResult;

            if (string.IsNullOrWhiteSpace(utterance))
            {
                luisResult = DefaultIntent(utterance);
            }
            else
            {
                var (intents, entities) = ExtractIntentsAndEntities(utterance);
                luisResult = new TimeSurveyBotLuis
                {
                    Text = utterance,
                    Intents = intents,
                    Entities = entities
                };
            }

            // TODO Verify, it works, but we should do it better
            return ConvertToT<T>(luisResult);
        }

        private static T ConvertToT<T>(TimeSurveyBotLuis luisResult) where T : IRecognizerConvert, new()
        {
            // TODO maybe substitute default with new T()
            return (object) luisResult is T ? (T)(object) luisResult : default;
        }

        private static TimeSurveyBotLuis DefaultIntent(string? utterance = null)
        {
            return new TimeSurveyBotLuis
            {
                Text = utterance ?? "",
                Intents = new Dictionary<TimeSurveyBotLuis.Intent, IntentScore>
                    { { TimeSurveyBotLuis.Intent.None, new IntentScore { Score = 1.0 } } },
            };
        }

        private static (Dictionary<TimeSurveyBotLuis.Intent, IntentScore> intents, TimeSurveyBotLuis._Entities entities)
            ExtractIntentsAndEntities(string utterance)
        {
            int indexOfColon = utterance.IndexOf(":", StringComparison.Ordinal);
            
            string intentString = indexOfColon == -1 ? utterance : utterance[..indexOfColon];
            var intent = Enum.TryParse(intentString, true, out TimeSurveyBotLuis.Intent result)
                ? result
                : TimeSurveyBotLuis.Intent.None;
            var intents = new Dictionary<TimeSurveyBotLuis.Intent, IntentScore>
                { { intent, new IntentScore { Score = 1.0 } } };

            var entities = indexOfColon != -1
                ? ExtractEntities(utterance[indexOfColon..].Split(","))  // TODO check when we have only one entity
                : new TimeSurveyBotLuis._Entities();

            return (intents, entities);
        }
        
        // TODO we can use a map of Intent: string[]
        // TODO foreach intent provide a converter string -> TimeSurveyBotLuis._Entities
        private static TimeSurveyBotLuis._Entities ExtractEntities(string[] entities)
        {
            return new TimeSurveyBotLuis._Entities();
        }
        
    }
}