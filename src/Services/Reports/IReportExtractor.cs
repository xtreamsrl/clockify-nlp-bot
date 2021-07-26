using Bot.Models;
using Luis;

namespace Bot.Services.Reports
{
    public interface IReportExtractor
    {
        string GetDateTimeInstance(TimeSurveyBotLuis._Entities._Instance entities);

        DateRange GetDateRangeFromTimePeriod(string timePeriodInstance);
    }

}