using System.Threading.Tasks;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Bot.Clockify
{
    public interface IFollowUpService
    {
        Task<string> SendFollowUpAsync(IBotFrameworkHttpAdapter adapter);
    }
}