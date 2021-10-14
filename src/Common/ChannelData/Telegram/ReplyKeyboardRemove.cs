using Newtonsoft.Json;

namespace Bot.Common.ChannelData.Telegram
{
    public class ReplyKeyboardRemove
    {
        [JsonProperty("remove_keyboard")]
        public const bool RemoveKeyboard = true;

        [JsonProperty("selective", NullValueHandling = NullValueHandling.Ignore)] public bool? Selective { get; }

        public ReplyKeyboardRemove(bool? selective = null)
        {
            Selective = selective;
        }
    }
}