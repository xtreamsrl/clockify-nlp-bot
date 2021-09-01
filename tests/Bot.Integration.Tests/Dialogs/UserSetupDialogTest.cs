using Bot.Clockify;
using Bot.Clockify.Client;
using Bot.Data;
using Bot.States;
using FluentAssertions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;
using static Bot.Integration.Tests.Clockify.ClockifyConsts;

namespace Bot.Integration.Tests.Dialogs
{
    public class UserSetupDialogTest
    {
        [Fact]
        private async void UserSetupDialogMainFlowTest()
        {
            var userState = new UserState(new MemoryStorage());
            var clockifyService = new ClockifyService(new ClockifyClientFactory());
            var clockifyMessageSource = new TestClockifyMessageSource();
            var mockTokenRepository = new Mock<ITokenRepository>();
            mockTokenRepository.Setup(r => r.WriteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TokenData("id", ClockifyApiKey));

            var dialog = new ClockifySetupDialog(userState, clockifyService, mockTokenRepository.Object, clockifyMessageSource);
            var dialogTestClient = new DialogTestClient(Channels.Telegram, dialog);

            var reply = await dialogTestClient.SendActivityAsync<IMessageActivity>("ciao");
            reply.Text.Should().Be(clockifyMessageSource.SetupRequest);

            reply = await dialogTestClient.SendActivityAsync<IMessageActivity>(InvalidApiKey);
            reply.Text.Should().Be(clockifyMessageSource.SetupReject);

            reply = await dialogTestClient.SendActivityAsync<IMessageActivity>(ClockifyApiKey);
            reply.Text.Should().Be(clockifyMessageSource.SetupFeedback);

            var userProfile = await userState.CreateProperty<UserProfile>("UserProfile")
                .GetAsync(dialogTestClient.DialogContext.Context);

            userProfile.ClockifyTokenId.Should().NotBeNull();

            // TODO: How do we test the end of the dialog?
        }

        [Fact]
        private async void UserSetupDialog_ClockifyIsDown_ShouldReturnMessageError()
        {
            var userState = new UserState(new MemoryStorage());
            var clockifyService = new Mock<IClockifyService>();
            var clockifyMessageSource = new TestClockifyMessageSource();
            clockifyService
                .Setup(service => service.GetCurrentUserAsync(It.IsAny<string>()))
                .ThrowsAsync(new ErrorResponseException("unable to get current user"));
            var mockTokenRepository = new Mock<ITokenRepository>();

            var dialog = new ClockifySetupDialog(userState, clockifyService.Object, mockTokenRepository.Object, clockifyMessageSource);
            var dialogTestClient = new DialogTestClient(Channels.Telegram, dialog);

            var reply = await dialogTestClient.SendActivityAsync<IMessageActivity>("ciao");
            reply.Text.Should().Be(clockifyMessageSource.SetupRequest);

            reply = await dialogTestClient.SendActivityAsync<IMessageActivity>(ClockifyApiKey);

            reply.Text.Should().Be(clockifyMessageSource.SetupReject);
        }
    }

    internal class TestClockifyMessageSource : IClockifyMessageSource
    {
        public string SetupRequest => nameof(SetupRequest);
        public string SetupFeedback  => nameof(SetupFeedback);
        public string SetupReject  => nameof(SetupReject);
    }
}