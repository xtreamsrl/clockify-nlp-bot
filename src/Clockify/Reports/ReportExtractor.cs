using Bot.Common;

namespace Bot.Clockify.Reports
{
    public class ReportExtractor : IReportExtractor
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        
        public ReportExtractor(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        // TODO from yesterday to today return correct dates at 00:00 and clockify doesn't return time entries for today 
        public DateRange GetDateRangeFromTimePeriod(string timePeriodInstance)
        {
            var dateRange = TextToDateRangeService.Convert(timePeriodInstance, _dateTimeProvider.DateTimeNow());
            if (dateRange == null)
            {
                throw new InvalidDateRangeException($"Cannot parse {timePeriodInstance} date range.");
            }

            return dateRange.Value;
        }
    }
}