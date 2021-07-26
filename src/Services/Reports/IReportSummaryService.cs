using System.Threading.Tasks;
using Bot.Models;
using Bot.States;

namespace Bot.Services.Reports
{
    public interface IReportSummaryService
    {
        Task<string> Summary(UserProfile userProfile, DateRange dateRange);
    }
}