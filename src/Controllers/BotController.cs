using System.Threading.Tasks;
using Bot.Remind;
using Bot.Security;
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
        private readonly IRemindService _smartWorkingRemindService;

        public BotController(IBotFrameworkHttpAdapter adapter, IProactiveBotApiKeyValidator proactiveBotApiKeyValidator, 
            IBot bot, IRemindServiceResolver remindServiceResolver)
        {
            _bot = bot;
            _adapter = adapter;
            _proactiveBotApiKeyValidator = proactiveBotApiKeyValidator;
            _smartWorkingRemindService = remindServiceResolver.Resolve("SmartWorking");
        }

        [Route("api/messages")]
        [HttpPost]
        [HttpGet]
        public async Task PostAsync()
        {
            await _adapter.ProcessAsync(Request, Response, _bot);
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