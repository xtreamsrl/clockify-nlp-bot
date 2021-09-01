namespace Bot.Clockify
{
    public interface IClockifyMessageSource
    {
        string SetupRequest { get; }
        string SetupFeedback { get; }
        string SetupReject { get; }
    }
    
}