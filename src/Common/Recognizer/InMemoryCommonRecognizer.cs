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
            var result = new T();
            result.Convert(await RecognizeInternalAsync(turnContext, cancellationToken).ConfigureAwait(false));
            return result;
        }

        private async Task<RecognizerResult> RecognizeInternalAsync(ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type != ActivityTypes.Message)
            {
                return null;
            }

            string? utterance = turnContext.Activity?.AsMessageActivity()?.Text;
            RecognizerResult recognizerResult;

            if (string.IsNullOrWhiteSpace(utterance))
            {
                recognizerResult = new RecognizerResult
                {
                    Text = utterance,
                    Intents = new Dictionary<string, IntentScore> { { string.Empty, new IntentScore { Score = 1.0 } } },
                    Entities = new JObject(),
                };
            }
            else
            {
                var (intents, entities) = ExtractIntentsAndEntities(utterance);
                recognizerResult = new RecognizerResult
                {
                    Text = utterance,
                    Intents = intents,
                    Entities = entities,
                };
            }

            return recognizerResult;
        }

        private static (Dictionary<string, IntentScore> intents, JObject entities) ExtractIntentsAndEntities(
            string utterance)
        {
            int indexOfColon = utterance.IndexOf(":", StringComparison.Ordinal);
            
            string intent = utterance[..indexOfColon];
            var intents = new Dictionary<string, IntentScore> { { intent, new IntentScore { Score = 1.0 } } };
            
            string[] entitiesStrings = utterance[indexOfColon..].Split(",");
            JObject entities = ExtractEntities(entitiesStrings);
            
            return (intents, entities);
        }

        private static JObject ExtractEntities(string[] entitiesStrings)
        {
            
            throw new NotImplementedException();
        }
    }
}