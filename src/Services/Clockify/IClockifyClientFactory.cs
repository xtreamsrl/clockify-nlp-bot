namespace Bot.Services.Clockify
{
    public interface IClockifyClientFactory
    {
        IClockifyClient CreateClient(string apiKey);
    }
}