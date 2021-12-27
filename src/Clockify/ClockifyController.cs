using System.Linq;
using System.Threading.Tasks;
using Bot.Remind;
using Bot.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Bot.Clockify
{
    [ApiController]
    public class ClockifyController : ControllerBase
    {
        private readonly IProactiveBotApiKeyValidator _proactiveBotApiKeyValidator;
        private readonly ISpecificRemindService _entryFillRemindService;
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IFollowUpService _followUpService;

        public ClockifyController(IBotFrameworkHttpAdapter adapter,
            IProactiveBotApiKeyValidator proactiveBotApiKeyValidator,
            ISpecificRemindServiceResolver specificRemindServiceResolver,
            IFollowUpService followUpService)
        {
            _adapter = adapter;
            _proactiveBotApiKeyValidator = proactiveBotApiKeyValidator;
            _followUpService = followUpService;
            _entryFillRemindService = specificRemindServiceResolver.Resolve("EntryFill");
        }

        [Route("api/timesheet/remind")]
        [HttpGet]
        public async Task<string> GetTimesheetRemindAsync()
        {
            string apiToken = ProactiveApiKeyUtil.Extract(Request);
            _proactiveBotApiKeyValidator.Validate(apiToken);

            var typesToRemind = SpecificRemindService.ReminderType.YesterdayReminder |
                                SpecificRemindService.ReminderType.TodayReminder;

            //Check for additional query parameters. If there are available, we will only remind those reminders
            if (Request.Query.ContainsKey("type"))
            {
                var requestedReminderTypes = Request.Query["type"];
                //Check for the specific teminder types
                typesToRemind = SpecificRemindService.ReminderType.NoReminder;
                if (requestedReminderTypes.Contains("yesterday"))
                    typesToRemind |= SpecificRemindService.ReminderType.YesterdayReminder;
                
                if (requestedReminderTypes.Contains("today"))
                    typesToRemind |= SpecificRemindService.ReminderType.TodayReminder;
            }

            return await _entryFillRemindService.SendReminderAsync(_adapter, typesToRemind);
        }

        [Route("api/follow-up")]
        [HttpPost]
        public async Task<string> SendFollowUpAsync()
        {
            string apiToken = ProactiveApiKeyUtil.Extract(Request);
            _proactiveBotApiKeyValidator.Validate(apiToken);

            var followedUsers = await _followUpService.SendFollowUpAsync((BotAdapter)_adapter);

            return $"Sent follow up to {followedUsers.Count} users";
        }
    }
}