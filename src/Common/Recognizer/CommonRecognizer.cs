using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;

namespace Bot.Common.Recognizer
{
    public sealed class CommonRecognizer : IRecognizer
    {
        private readonly LuisRecognizer _recognizer;

        public CommonRecognizer(LuisRecognizer recognizer)
        {
            _recognizer = recognizer;
        }

        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext,
            CancellationToken cancellationToken)
            => await _recognizer.RecognizeAsync(turnContext, cancellationToken);

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken)
            where T : IRecognizerConvert, new()
            => await _recognizer.RecognizeAsync<T>(turnContext, cancellationToken);
        
    }
}