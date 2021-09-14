using System.Threading;
using System.Threading.Tasks;
using Bot.Common.Recognizer;
using Bot.States;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Supports
{
    /// <summary>
    /// A component to handle a conversation turn as part of a chain 
    /// </summary>
    public interface IBotHandler
    {
        /// <summary>
        /// Handle a conversation turn. The returned value indicates whether handling should be considered as concluded
        /// or it should continue down the chain in case of subsequent handlers.
        /// </summary>
        /// <param name="turnContext">The turn context to handle</param>
        /// <param name="cancellationToken">A cancellation token for the turn</param>
        /// <param name="userProfile">The user profile of the conversation</param>
        /// <param name="luisResult">The Luis recognition result</param>
        /// <returns>A flag that indicates whether handling should stop or continue down the chain</returns>
        Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken, UserProfile userProfile, TimeSurveyBotLuis? luisResult = null);

        /// <summary>
        /// Get the set of dialogs managed by this handler. This allows the caller to know whether this handler has a
        /// non concluded dialog in progress
        /// </summary>
        /// <returns>The set of dialogs managed by this handler. This set can be empty</returns>
        DialogSet GetDialogSet();
    }
}