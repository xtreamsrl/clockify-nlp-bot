using System.Threading;
using System.Threading.Tasks;
using Bot.Clockify;
using Bot.States;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Bot.Supports
{
    public class UtilityHandler : IBotHandler
    {
        private readonly LuisRecognizerProxy _luisRecognizer;
        private readonly DialogSet _dialogSet;

        public UtilityHandler(ConversationState conversationState, LuisRecognizerProxy luisRecognizer)
        {
            IStatePropertyAccessor<DialogState> dialogState = conversationState.CreateProperty<DialogState>("UtilityDialogState");
            _luisRecognizer = luisRecognizer;
            _dialogSet = new DialogSet(dialogState);
        }

        public async Task<bool> Handle(ITurnContext turnContext, CancellationToken cancellationToken,
            UserProfile userProfile)
        {
            if (userProfile.ClockifyToken == null && userProfile.ClockifyTokenId == null)
            {
                await ExplainBot(turnContext, cancellationToken);
            }

            var (topIntent, _) =
                await _luisRecognizer.RecognizeAsyncIntent(turnContext, cancellationToken);

            switch (topIntent)
            {
                case TimeSurveyBotLuis.Intent.Thanks:
                {
                    const string message = "You're welcome ❤";
                    await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                    return true;
                }
                case TimeSurveyBotLuis.Intent.Insult:
                {
                    const string message = "Language, please...";
                    await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                    return true;
                }
                case TimeSurveyBotLuis.Intent.Utilities_Help:
                {
                    await ExplainBot(turnContext, cancellationToken);
                    return true;
                }
                default:
                    return false;
            }
        }

        public DialogSet GetDialogSet() => _dialogSet;

        private static async Task ExplainBot(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(
                MessageFactory.Text(
                    "Hi, welcome! I am a natural language processing powered bot 🤖 born to help folks " +
                    "report their time on Clockify efficiently and naturally.\n I started as an internal tool for " +
                    "[xtream](https://xtreamers.io?utm_source=" + turnContext.Activity.ChannelId +
                    "utm_medium=bot&utm_campaign=clockify_bot&utm_content=welcome_message) and then I was rolled out to everyone. " +
                    "Who am I for? \n" +
                    "- people who tend to fill entries on Clockify manually, and get bored doing so\n" +
                    "- managers who want to improve data quality on time reports, turning them into actionable insights\n" +
                    "- people who tend to do this due diligence while on the subway, brushing their teeth, or while on the 🚽 (no shame in that)"),
                cancellationToken);

            await turnContext.SendActivityAsync(
                MessageFactory.Text(
                    "How do I work? As of now I'm pretty simple, and I'll do for you 3 things:\n" +
                    "- I'll remind you to fill your entries, every day from 5.30pm onwards. I'll stop " +
                    "when I see enough hours in there (6+) or when you tell me to (just say `stop`)\n" +
                    "- at any time you like, you can tell me some work you did. I'll gladly handle something like " +
                    "\"add 2 hours for presales\", or similar. You can tell me how much time as you prefer (6h, 10 minutes, " +
                    "half day, ...) and you can refer to the project quite freely, I should be smart enough to cover for " +
                    "misspellings, partial names and so on. **Memo:** You can only insert time for the current day.\n" +
                    "- if you don't remember what time you reported already, just ask me. Something like " +
                    "\"my work this week?\". Again, feel free to ask for any time range you like, I love a challenge 🙃"),
                cancellationToken);

            await turnContext.SendActivityAsync(
                MessageFactory.Text(
                    "I was built for people who needed me, nothing more. I " +
                    "don't store any of your data, only your access token for Clockify, which you can revoke or change with a click. " +
                    "And because I value your trust, I store it securely on Microsoft Azure dedicated Key Vault. Don't just take my word for " +
                    "granted, you can check my [source code](https://github.com/xtreamsrl/clockify-nlp-bot) as well, " +
                    "it's open and public! That being said, enjoy chatting with me, and I hope I'll help: for any trouble, " +
                    "open an [issue](https://github.com/xtreamsrl/clockify-nlp-bot/issues) on GitHub, or if you're not of " +
                    "the geeky persuasion just write my creators at oss@xtreamers.io "),
                cancellationToken);

            await turnContext.SendActivityAsync(
                MessageFactory.Text(
                    "Ah, one more thing. I only speak English 💂 for now, but I'm studying other languages 📖😉"),
                cancellationToken);
        }
    }
}