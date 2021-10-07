namespace Bot.Common.ChannelData.Telegram
{
    public class SendMessage: ChannelData<SendMessageParameters>
    {
        private static string Method => "sendMessage";
        
        public SendMessage(SendMessageParameters parameters) : base(Method, parameters)
        {
        }
    }
}