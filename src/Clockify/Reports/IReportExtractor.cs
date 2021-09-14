namespace Bot.Clockify.Reports
{
    public interface IReportExtractor
    {
        DateRange GetDateRangeFromTimePeriod(string timePeriodInstance);
    }

}