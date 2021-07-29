namespace Bot.Services.Clockify
{
    public class ClockifyClientFactory: IClockifyClientFactory
    {
        public IClockifyClient CreateClient(string apiKey)
        {
            return new RichClockifyClient(apiKey);
        }
    }
}