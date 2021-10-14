using Newtonsoft.Json;
namespace Bot.Common.ChannelData.Telegram
{
    public class SendMessageParameters
    {
        
        [JsonProperty("text")]
        public string Text { get; }

        [JsonProperty("reply_markup")] public ReplyKeyboardRemove ReplyMarkup { get; }

        public SendMessageParameters(string text, ReplyKeyboardRemove replyMarkup)
        {
            Text = text;
            ReplyMarkup = replyMarkup;
        }
    }
}