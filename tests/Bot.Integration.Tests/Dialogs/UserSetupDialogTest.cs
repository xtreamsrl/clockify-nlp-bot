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
        private const string SetupRequest = "SetupRequest";
        private const string SetupReject = "SetupReject";
        private const string SetupFeedback = "SetupFeedback";
        
        [Fact]
        private async void UserSetupDialogMainFlowTest()
        {
            var userState = new UserState(new MemoryStorage());
            var clockifyService = new ClockifyService(new ClockifyClientFactory());
            var clockifyMessageSource = new Mock<IClockifyMessageSource>();
            clockifyMessageSource.Setup(c => c.SetupRequest).Returns(SetupRequest);
            clockifyMessageSource.Setup(c => c.SetupReject).Returns(SetupReject);
            clockifyMessageSource.Setup(c => c.SetupFeedback).Returns(SetupFeedback);
            var mockTokenRepository = new Mock<ITokenRepository>();
            mockTokenRepository.Setup(r => r.WriteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TokenData("id", ClockifyApiKey));

            var dialog = new ClockifySetupDialog(userState, clockifyService, mockTokenRepository.Object,
                clockifyMessageSource.Object);
            var dialogTestClient = new DialogTestClient(Channels.Telegram, dialog);

            var reply = await dialogTestClient.SendActivityAsync<IMessageActivity>("ciao");
            reply.Text.Should().Be(SetupRequest);

            reply = await dialogTestClient.SendActivityAsync<IMessageActivity>(InvalidApiKey);
            reply.Text.Should().Be(SetupReject);

            reply = await dialogTestClient.SendActivityAsync<IMessageActivity>(ClockifyApiKey);
            reply.Text.Should().Be(SetupFeedback);

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
            clockifyService
                .Setup(service => service.GetCurrentUserAsync(It.IsAny<string>()))
                .ThrowsAsync(new ErrorResponseException("unable to get current user"));
            var clockifyMessageSource = new Mock<IClockifyMessageSource>();
            clockifyMessageSource.Setup(c => c.SetupRequest).Returns(SetupRequest);
            clockifyMessageSource.Setup(c => c.SetupReject).Returns(SetupReject);
            var mockTokenRepository = new Mock<ITokenRepository>();

            var dialog = new ClockifySetupDialog(userState, clockifyService.Object, mockTokenRepository.Object,
                clockifyMessageSource.Object);
            var dialogTestClient = new DialogTestClient(Channels.Telegram, dialog);

            var reply = await dialogTestClient.SendActivityAsync<IMessageActivity>("ciao");
            reply.Text.Should().Be(SetupRequest);

            reply = await dialogTestClient.SendActivityAsync<IMessageActivity>(ClockifyApiKey);

            reply.Text.Should().Be(SetupReject);
        }
    }
}