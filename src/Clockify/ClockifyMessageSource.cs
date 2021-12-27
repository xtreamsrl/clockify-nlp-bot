using System;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace Bot.Clockify
{
    public class ClockifyMessageSource : IClockifyMessageSource
    {
        private static readonly Random Rnd = new Random();
        
        private readonly IStringLocalizer<ClockifyMessageSource> _localizer;

        public ClockifyMessageSource(IStringLocalizer<ClockifyMessageSource> localizer)
        {
            _localizer = localizer;
        }

        public string SetupRequest => GetString(nameof(SetupRequest));
        public string SetupFeedback => GetString(nameof(SetupFeedback));
        public string SetupReject => GetString(nameof(SetupReject));

        public string TaskCreationError => GetString(nameof(TaskCreationError));
        public string TaskUnrecognized => GetString(nameof(TaskUnrecognized));
        public string TaskCreation => GetString(nameof(TaskCreation));
        public string TaskAbort => GetString(nameof(TaskAbort));
        public string AddEntryFeedback => GetString(nameof(AddEntryFeedback));
        public string SetWorkingHoursFeedback => GetString(nameof(SetWorkingHoursFeedback));
        public string SetWorkingHoursUnchangedFeedback => GetString(nameof(SetWorkingHoursUnchangedFeedback));
        public string EntryFillUnderstandingError => GetString(nameof(EntryFillUnderstandingError));
        public string AmbiguousProjectError => GetString(nameof(AmbiguousProjectError));
        public string ProjectUnrecognized => GetString(nameof(ProjectUnrecognized));
        public string TaskUnrecognizedRetry => GetString(nameof(TaskUnrecognizedRetry));
        public string TaskSelectionQuestion => GetString(nameof(TaskSelectionQuestion));
        public string NewTask => GetString(nameof(NewTask));
        public string No => GetString(nameof(No));

        public string ReportDateRangeError => GetString(nameof(ReportDateRangeError));
        public string ReportWorkedPeriodUnrecognized => GetString(nameof(ReportWorkedPeriodUnrecognized));
        public string ReportTotalHours => GetString(nameof(ReportTotalHours));
        public string ReportNoWork => GetString(nameof(ReportNoWork));
        public string ReportWork => GetString(nameof(ReportWork));
        public string ReportDateRangeExceedOneYear => GetString(nameof(ReportDateRangeExceedOneYear));

        public string FollowUp => GetString(nameof(FollowUp));

        public string RemindStoppedAlready => GetString(nameof(RemindStoppedAlready));
        public string RemindStopAnswer => GetString(nameof(RemindStopAnswer));

        public string RemindEntryFill => GetString(nameof(RemindEntryFill));

        public string RemindEntryFillYesterday => GetString(nameof(RemindEntryFillYesterday));

        private string GetString(string name)
        {
            if (!_localizer[name].ResourceNotFound) return _localizer[name].Value;
            string? key = _localizer.GetAllStrings()
                .Select(s => s.Name)
                .Where(k => k.StartsWith(name))
                .OrderBy(_ => Rnd.Next())
                .Last();
            return _localizer[key].Value;
        }
    }
}