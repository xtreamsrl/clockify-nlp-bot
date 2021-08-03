namespace Bot.Clockify.Client
{
    public interface IClockifyClientFactory
    {
        IClockifyClient CreateClient(string apiKey);
    }
}