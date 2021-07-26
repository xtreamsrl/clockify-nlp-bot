using System.Threading.Tasks;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Tasks;

namespace Bot.Services.TimeEntries
{
    public interface ITimeEntryStoreService
    {
        public Task<double> AddTimeEntries(string clockifyToken, ProjectDtoImpl project, TaskDto? task, double minutes);
    }
}