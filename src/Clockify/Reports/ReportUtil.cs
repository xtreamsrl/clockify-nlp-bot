using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bot.Clockify.Models;

namespace Bot.Clockify.Reports
{
    public static class ReportUtil
    {
        public static IEnumerable<ReportEntry> ConvertToReportEntries(IEnumerable<HydratedTimeEntryDo> timeEntries)
        {
            var reportEntries =
                from timeEntry in timeEntries
                group timeEntry by new
                {
                    ProjectName = timeEntry.Project != null ? timeEntry.Project.Name : "",
                    TaskName = timeEntry.Task != null ? timeEntry.Task.Name : ""
                }
                into groupedTimeEntry
                where groupedTimeEntry.Key.ProjectName != string.Empty
                select new ReportEntry(
                    groupedTimeEntry.Key.ProjectName,
                    groupedTimeEntry.Key.TaskName,
                    groupedTimeEntry.Sum(DurationInDecimal)
                );

            return reportEntries;
        }

        public static string SummaryForReportEntries(IEnumerable<ReportEntry> reportEntries)
        {
            var summary = new StringBuilder();
            var sortedEntries = 
                reportEntries.OrderBy(entry => entry.Project).ThenBy(entry => entry.Task);
            foreach (var reportEntry in sortedEntries)
            {
                summary.Append(
                    reportEntry.Task != ""
                        ? $"\n- **{reportEntry.Project}** - {reportEntry.Task}: {FormatDuration(reportEntry.Hours)}"
                        : $"\n- **{reportEntry.Project}**: {FormatDuration(reportEntry.Hours)}");
            }
            return summary.ToString();
        }

        public static string FormatDuration(float duration)
        {
            double days = duration / 8.0;
            return $"{days:0.00}d";
        }

        private static float DurationInDecimal(HydratedTimeEntryDo hydratedTimeEntry)
        {
            var end = hydratedTimeEntry.TimeInterval.End;
            var start = hydratedTimeEntry.TimeInterval.Start;

            if (start == null || end == null) return 0.0f;

            return (float) end.Value.Subtract(start.Value).TotalSeconds / 3600;
        }
    }
}