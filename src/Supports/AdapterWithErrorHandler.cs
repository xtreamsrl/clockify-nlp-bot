using System;
using Bot.Common;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bot.Supports
{
    public class AdapterWithErrorHandler : CloudAdapter
    {
        
        public AdapterWithErrorHandler(BotFrameworkAuthentication auth, IHostEnvironment environment, IConfiguration configuration,
            ILogger<BotFrameworkHttpAdapter> logger, ICommonMessageSource messageSource,
            ConversationState? conversationState = default) : base(auth, logger)
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
                
                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e,
                            "Exception caught on attempting to Delete ConversationState : {ExMessage}", e.Message);
                    }
                }
        
                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message,
                    "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}