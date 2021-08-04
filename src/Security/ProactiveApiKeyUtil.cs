using Microsoft.AspNetCore.Http;

namespace Bot.Security
{
    public static class ProactiveApiKeyUtil
    {
        public const string ProactiveBotApiKey = "ProactiveBotApiKey";
        public static string Extract(HttpRequest request)
        {
            return request.Headers[ProactiveBotApiKey].ToString() ?? "";
        }
    }
}