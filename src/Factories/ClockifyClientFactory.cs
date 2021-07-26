using Bot.Services;

namespace Bot.Factories
{
    public static class ClockifyClientFactory
    {
        public static IClockifyClient CreateClient(string apiKey)
        {
            return new RichClockifyClient(apiKey);
        }
    }
}