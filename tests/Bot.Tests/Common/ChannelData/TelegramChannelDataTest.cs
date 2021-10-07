using Bot.Common.ChannelData.Telegram;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Bot.Tests.Common.ChannelData
{
    public class TelegramChannelDataTest
    {
        [Fact]
        public void SendMessage_ReplyKeyboardRemove_ProperlySerialize()
        {
            const string expectedChannelData =
                "{\"method\":\"sendMessage\",\"parameters\":{\"text\":\"Test message text!\",\"reply_markup\":{\"remove_keyboard\":true}}}";
            const string text = "Test message text!";
            var sendMessageParams = new SendMessageParameters(text, new ReplyKeyboardRemove());
            var channelData = new SendMessage(sendMessageParams);
            var serializedChannelData = JsonConvert.SerializeObject(channelData);
            serializedChannelData.Should().Be(expectedChannelData);
        }

        [Fact]
        public void SendMessage_ReplyKeyboardRemoveSelective_ProperlySerialize()
        {
            const string expectedChannelData =
                "{\"method\":\"sendMessage\",\"parameters\":{\"text\":\"Test message text!\",\"reply_markup\":{\"remove_keyboard\":true,\"selective\":true}}}";
            const string text = "Test message text!";
            var sendMessageParams = new SendMessageParameters(text, new ReplyKeyboardRemove(true));
            var channelData = new SendMessage(sendMessageParams);
            var serializedChannelData = JsonConvert.SerializeObject(channelData);
            serializedChannelData.Should().Be(expectedChannelData);
        }
    }
}