namespace Bot.Services.Clockify
{
    public static class ClockifyClientFactory
    {
        public static IClockifyClient CreateClient(string apiKey)
        {
            return new RichClockifyClient(apiKey);
        }
    }
}