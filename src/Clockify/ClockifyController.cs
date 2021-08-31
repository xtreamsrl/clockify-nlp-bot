using System.Threading.Tasks;
using Bot.Remind;
using Bot.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Bot.Clockify
{
    [ApiController]
    public class ClockifyController : ControllerBase
    {
        private readonly IProactiveBotApiKeyValidator _proactiveBotApiKeyValidator;
        private readonly IRemindService _entryFillRemindService;
        private readonly IBotFrameworkHttpAdapter _adapter;

        public ClockifyController(IBotFrameworkHttpAdapter adapter,
            IProactiveBotApiKeyValidator proactiveBotApiKeyValidator, IRemindServiceResolver remindServiceResolver)
        {
            _adapter = adapter;
            _proactiveBotApiKeyValidator = proactiveBotApiKeyValidator;
            _entryFillRemindService = remindServiceResolver.Resolve("EntryFill");
        }

        [Route("api/timesheet/remind")]
        [HttpGet]
        public async Task<string> GetTimesheetRemindAsync()
        {
            string apiToken = ProactiveApiKeyUtil.Extract(Request);
            _proactiveBotApiKeyValidator.Validate(apiToken);

            return await _entryFillRemindService.SendReminderAsync(_adapter);
        }
    }
}