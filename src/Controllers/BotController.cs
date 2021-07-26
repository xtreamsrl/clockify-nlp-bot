using System.Threading.Tasks;
using Bot.Security;
using Bot.Services.Reminds;
using Bot.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Bot.Controllers
{

    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IProactiveBotApiKeyValidator _proactiveBotApiKeyValidator;
        private readonly IBot _bot;
        private readonly IEntryFillRemindService _entryFillRemindService;
        private readonly ISmartWorkingRemindService _smartWorkingRemindService;

        public BotController(IBotFrameworkHttpAdapter adapter, IProactiveBotApiKeyValidator proactiveBotApiKeyValidator, 
            IBot bot, IEntryFillRemindService entryFillRemindService, ISmartWorkingRemindService smartWorkingRemindService)
        {
            _bot = bot;
            _adapter = adapter;
            _proactiveBotApiKeyValidator = proactiveBotApiKeyValidator;
            _entryFillRemindService = entryFillRemindService;
            _smartWorkingRemindService = smartWorkingRemindService;
        }

        [Route("api/messages")]
        [HttpPost]
        [HttpGet]
        public async Task PostAsync()
        {
            await _adapter.ProcessAsync(Request, Response, _bot);
        }
        
        [Route("api/timesheet/remind")]
        [HttpGet]
        public async Task<string> GetTimesheetRemindAsync()
        {
            string apiToken = ProactiveApiKeyUtil.Extract(Request);
            _proactiveBotApiKeyValidator.Validate(apiToken);
            
            return await _entryFillRemindService.SendReminderAsync(_adapter);
        }
        
        [Route("api/flextime/remind")]
        [HttpGet]
        public async Task<string> GetSmartWorkingRemindAsync()
        {
            string apiToken = ProactiveApiKeyUtil.Extract(Request);
            _proactiveBotApiKeyValidator.Validate(apiToken);
            
            return await _smartWorkingRemindService.SendReminderAsync(_adapter);
        }
    }
}