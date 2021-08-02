using System;
using System.Collections.Generic;
using Bot.Clockify.Fill;
using Bot.Clockify.Reports;
using Luis;

namespace Bot.Clockify
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