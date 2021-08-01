using System;
using System.Threading;
using System.Threading.Tasks;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace Bot
{
    public static class IntentManager
    {
        public static async Task<bool> HandleIntent(DialogContext dialogContext,
            TimeSurveyBotLuis.Intent intent,
            CancellationToken cancellationToken,
            ITurnContext<IMessageActivity> turnContext,
            TimeSurveyBotLuis._Entities._Instance entities,
            DialogSet dialogSet
        )
        {
            
            // TODO add integration test and put this condition inside the appropriate switch
            // Sometimes the intent is not recognized properly
            if (entities.datetime != null && entities.WorkedEntity != null)
            {
                var fillDialog = dialogSet.Find(TimeSurveyBotLuis.Intent.Fill.ToString());
                await dialogContext.BeginDialogAsync(fillDialog.Id, entities, cancellationToken);
                return true;
            }
            
            switch (intent)
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
                case TimeSurveyBotLuis.Intent.Report:
                    var reportDialog = dialogSet.Find(TimeSurveyBotLuis.Intent.Report.ToString());
                    await dialogContext.BeginDialogAsync(reportDialog.Id, entities, cancellationToken);
                    return true;
                case TimeSurveyBotLuis.Intent.Fill:
                    var fillDialog = dialogSet.Find(TimeSurveyBotLuis.Intent.Fill.ToString());
                    await dialogContext.BeginDialogAsync(fillDialog.Id, entities, cancellationToken);
                    return true;
                case TimeSurveyBotLuis.Intent.FillAsYesterday:
                    // Unused
                    break;
                case TimeSurveyBotLuis.Intent.None:
                    break;
                case TimeSurveyBotLuis.Intent.Utilities_Help:
                {
                    const string message = "I can sure help you. This is what I can do:\n" +
                                           "- **reporting**: ask me to give you insight about a reporting period, and surprisingly enough I will! " +
                                           "For example, ask me *how much did I work last week?' and I'll give you all needed info\n\n" +
                                           "- **insertion**: feel like adding some entries? just tell me! For example, say to me *add 15 minutes on " +
                                           "r&d* and I will add it to today's time sheet.\n\n\n" +
                                           "Working with multiple workspaces? Don't worry, I got you covered";
                    await turnContext.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
                    return true;
                }
                case TimeSurveyBotLuis.Intent.Utilities_Stop:
                    var stopReminderDialog = dialogSet.Find(TimeSurveyBotLuis.Intent.Utilities_Stop.ToString());
                    await dialogContext.BeginDialogAsync(stopReminderDialog.Id, entities, cancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(intent), intent, null);
            }
            return false;
        }
    }
}