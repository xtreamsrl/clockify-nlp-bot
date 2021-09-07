namespace Bot.Integration.Tests.Clockify.Supports
{
    public class ClientReq
    {
        public ClientReq(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}