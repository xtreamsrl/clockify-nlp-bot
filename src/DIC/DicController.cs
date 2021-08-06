using System.Threading.Tasks;
using Bot.Remind;
using Bot.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace Bot.DIC
{
    [ApiController]
    public class DicController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly IProactiveBotApiKeyValidator _proactiveBotApiKeyValidator;
        private readonly IRemindService _smartWorkingRemindService;

        public DicController(IBotFrameworkHttpAdapter adapter, IProactiveBotApiKeyValidator proactiveBotApiKeyValidator,
            IRemindServiceResolver remindServiceResolver)
        {
            _adapter = adapter;
            _proactiveBotApiKeyValidator = proactiveBotApiKeyValidator;
            _smartWorkingRemindService = remindServiceResolver.Resolve("SmartWorking");
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