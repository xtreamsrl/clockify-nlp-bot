using Bot.Common;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bot.Supports
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(IHostEnvironment environment, IConfiguration configuration,
            ILogger<BotFrameworkHttpAdapter> logger, ICommonMessageSource messageSource) : base(configuration, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application
                logger.LogError(exception, "[OnTurnError] unhandled error : {ExMessage}", exception.Message);

                if (environment.IsDevelopment())
                {
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text($"Encountered exception: {exception.Message}"));
                    await turnContext.SendActivityAsync(MessageFactory.Text($"{exception.StackTrace ?? ""}"));
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(messageSource.GenericError));
                }

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message,
                    "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}