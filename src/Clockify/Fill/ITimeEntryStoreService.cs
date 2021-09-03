using System.Threading.Tasks;
using Bot.Clockify.Models;
using Clockify.Net.Models.Tasks;

namespace Bot.Clockify.Fill
{
    public interface ITimeEntryStoreService
    {
        public Task<double> AddTimeEntries(string clockifyToken, ProjectDo project, TaskDo? task, double minutes);
    }
}