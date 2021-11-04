using System.Threading.Tasks;
using Bot.Remind;
using Bot.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Bot.Clockify
{
    [ApiController]
    public class ClockifyController : ControllerBase
    {
        private readonly IProactiveBotApiKeyValidator _proactiveBotApiKeyValidator;
        private readonly IRemindService _entryFillRemindService;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IFollowUpService _followUpService;

        public ClockifyController(IBotFrameworkHttpAdapter adapter,
            IProactiveBotApiKeyValidator proactiveBotApiKeyValidator, IRemindServiceResolver remindServiceResolver,
            IFollowUpService followUpService)
        {
            _adapter = adapter;
            _proactiveBotApiKeyValidator = proactiveBotApiKeyValidator;
            _followUpService = followUpService;
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

        [Route("api/follow-up")]
        [HttpPost]
        public async Task<string> SendFollowUpAsync()
        {
            string apiToken = ProactiveApiKeyUtil.Extract(Request);
            _proactiveBotApiKeyValidator.Validate(apiToken);

            return await _followUpService.SendFollowUpAsync((BotAdapter)_adapter);
        }
    }
}