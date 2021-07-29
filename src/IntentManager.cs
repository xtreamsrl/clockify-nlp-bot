using System;
using System.Threading;
using System.Threading.Tasks;
using Bot.Dialogs;
using Bot.Services.Reports;
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
            EntryFillDialog fillDialog,
            ReportDialog reportDialog,
            StopReminderDialog stopReminderDialog
            )
        {
            
            // TODO add integration test and put this condition inside the appropriate switch
            // Sometimes the intent is not recognized properly
            if (entities.datetime != null && entities.WorkedEntity != null)
            {
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
                    await dialogContext.BeginDialogAsync(reportDialog.Id, entities, cancellationToken);
                    return true;
                case TimeSurveyBotLuis.Intent.Fill:
                    await dialogContext.BeginDialogAsync(fillDialog.Id, entities, cancellationToken);
                    return true;
                case TimeSurveyBotLuis.Intent.FillAsYesterday:
                    // Unused
                    break;
                case TimeSurveyBotLuis.Intent.None:
                    break;
                case TimeSurveyBotLuis.Intent.Utilities_Help:
                    // Unused
                    break;
                case TimeSurveyBotLuis.Intent.Utilities_Stop:
                    await dialogContext.BeginDialogAsync(stopReminderDialog.Id, entities, cancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(intent), intent, null);
            }
            return false;
        }
    }
}