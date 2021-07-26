namespace Bot.Security
{
    public interface IProactiveBotApiKeyValidator
    {
        void Validate(string clientApiKey);
    }
}