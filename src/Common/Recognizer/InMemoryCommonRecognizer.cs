using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using Newtonsoft.Json.Linq;

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
                ? ExtractEntities(intent, utterance[(indexOfColon+1)..].Split(",")) 
                : new TimeSurveyBotLuis._Entities();

            return (intents, entities);
        }
        
        private static TimeSurveyBotLuis._Entities ExtractEntities(TimeSurveyBotLuis.Intent intent, IEnumerable<string> entities)
        {
            IReadOnlyList<string>  polishedEntities = entities.Select(e => e.Trim(' ')).ToArray();
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

        private static TimeSurveyBotLuis._Entities FillEntitiesExtractor(IReadOnlyList<string> entities)
        {
            int numOfEntities = entities.Count;
            if (numOfEntities != 2)
            {
                throw new ArgumentException($"Fill intent require 2 entities but {numOfEntities} were found");
            }
            if (!EntityIsDuration(entities[0]))
            {
                throw new ArgumentException($"Entity [{entities[0]}] must be a duration");
            }

            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = entities[0],
                        Type = "builtin.datetimeV2.duration"
                    }
                },
                WorkedEntity = new []
                {
                    new InstanceData
                    {
                        Text = entities[1]
                    }
                }
            };
            
            return new TimeSurveyBotLuis._Entities
            {
                _instance = instances
            };
        }

        private static bool EntityIsDuration(string entity)
        {
            // TODO Find a way to make culture configurable.
            var recognizedDateTime = DateTimeRecognizer.RecognizeDateTime(entity, Culture.English).First();
            var resolvedDateTime = ((List<Dictionary<string, string>>)recognizedDateTime.Resolution["values"])[0];
            string dateTimeType = resolvedDateTime["type"];
            return dateTimeType.Equals("duration");
        }

        private static TimeSurveyBotLuis._Entities ReportEntitiesExtractor(IReadOnlyList<string> entities)
        {
            int numOfEntities = entities.Count;
            if (numOfEntities !=  1)
            {
                throw new ArgumentException($"Fill intent require 1 entity but {numOfEntities} were found");
            }
            if (!EntityIsDaterange(entities[0]))
            {
                throw new ArgumentException($"Entity [{entities[0]}] must be a daterange");
            }

            var instances = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = entities[0],
                        Type = "builtin.datetimeV2.daterange"
                    }
                }
            };
            
            return new TimeSurveyBotLuis._Entities
            {
                _instance = instances
            };
        }
        
        private static bool EntityIsDaterange(string entity)
        {
            // TODO Find a way to make culture configurable.
            var recognizedDateTime = DateTimeRecognizer.RecognizeDateTime(entity, Culture.English).First();
            var resolvedDateTime = ((List<Dictionary<string, string>>)recognizedDateTime.Resolution["values"])[0];
            string dateTimeType = resolvedDateTime["type"];
            return dateTimeType.Equals("daterange");
        }
    }
}