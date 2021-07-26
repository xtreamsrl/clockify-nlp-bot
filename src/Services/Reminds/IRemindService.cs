using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Bot.Services.Reminds
{
    public interface IRemindService
    {
        Task<string> SendReminderAsync(IBotFrameworkHttpAdapter adapter);
    }
}