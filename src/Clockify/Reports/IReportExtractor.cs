using Luis;

namespace Bot.Clockify.Reports
{
    public interface IReportExtractor
    {
        string GetDateTimeInstance(TimeSurveyBotLuis._Entities._Instance entities);

        DateRange GetDateRangeFromTimePeriod(string timePeriodInstance);
    }

}