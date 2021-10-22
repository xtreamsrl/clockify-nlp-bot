using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using static Bot.Common.Recognizer.EntitiesExtractors;

namespace Bot.Common.Recognizer
{
    public class InMemoryCommonRecognizer : IRecognizer
    {
        public Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                // it's kind of safe to return null, we won't use any result
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

        public Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
        {
            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                return Task.FromResult(ConvertToT<T>(DefaultIntent()));
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

            return Task.FromResult(ConvertToT<T>(luisResult));
        }

        private static T ConvertToT<T>(TimeSurveyBotLuis luisResult) where T : IRecognizerConvert, new()
        {
            // it's kind of safe to return null, we won't use any result
            return (object)luisResult is T ? (T)(object)luisResult : default;
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
                ? ExtractEntities(intent, utterance[(indexOfColon + 1)..].Split(","))
                : new TimeSurveyBotLuis._Entities();

            return (intents, entities);
        }

        private static TimeSurveyBotLuis._Entities ExtractEntities(TimeSurveyBotLuis.Intent intent,
            IEnumerable<string> entities)
        {
            IReadOnlyList<string> polishedEntities =
                entities.Select(e => e.Trim(' ')).Where(e => !string.IsNullOrEmpty(e)).ToArray();
            switch (intent)
            {
                case TimeSurveyBotLuis.Intent.Fill:
                {
                    return FillEntitiesExtractor(polishedEntities);
                }
                case TimeSurveyBotLuis.Intent.Report:
                    return ReportEntitiesExtractor(polishedEntities);
                default:
                    return new TimeSurveyBotLuis._Entities();
            }
        }
    }
}