using System;
using System.Collections.Generic;
using Luis;
using Bot.Services.Reports;

namespace Bot.Dialogs
{
    public static class DialogIdProvider
    {
        private static readonly IDictionary<Type, string> DialogIdDict = new Dictionary<Type, string>
        {
            {typeof(EntryFillDialog), TimeSurveyBotLuis.Intent.Fill.ToString()},
            {typeof(ReportDialog), TimeSurveyBotLuis.Intent.Report.ToString()},
            {typeof(StopReminderDialog), TimeSurveyBotLuis.Intent.Utilities_Stop.ToString()}
        };
        
        public static string GetDialogId(Type clazz)
        {
            if (!DialogIdDict.ContainsKey(clazz))
            {
                throw new ArgumentOutOfRangeException(nameof(clazz), clazz, "Dialog not registered");
            }

            return DialogIdDict[clazz];
        }
    }
}