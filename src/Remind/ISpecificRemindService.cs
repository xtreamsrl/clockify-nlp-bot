using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Bot.Remind
{
    public interface ISpecificRemindService
    {
        Task<string> SendReminderAsync(IBotFrameworkHttpAdapter adapter, SpecificRemindService.ReminderType reminderTypes);
    }
}