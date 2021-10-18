using System.Threading.Tasks;
using Bot.States;

namespace Bot.Clockify.Reports
{
    public interface IReportSummaryService
    {
        Task<string> Summary(string channel, UserProfile userProfile, DateRange dateRange);
    }
}