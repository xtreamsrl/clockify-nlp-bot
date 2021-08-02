namespace Bot.Clockify.Client
{
    public static class ClockifyClientFactory
    {
        public static IClockifyClient CreateClient(string apiKey)
        {
            return new RichClockifyClient(apiKey);
        }
    }
}