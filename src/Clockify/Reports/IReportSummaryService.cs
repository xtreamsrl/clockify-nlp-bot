using System.Threading.Tasks;
using Bot.States;

namespace Bot.Clockify.Reports
{
    public interface IReportSummaryService
    {
        Task<string> Summary(UserProfile userProfile, DateRange dateRange);
    }
}