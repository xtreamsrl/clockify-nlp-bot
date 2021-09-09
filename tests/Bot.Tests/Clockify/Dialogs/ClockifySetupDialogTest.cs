using System;
using Bot.Clockify;
using Bot.Clockify.Client;
using Bot.Clockify.Models;
using Bot.Data;
using Bot.States;
using FluentAssertions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Builder.Testing.XUnit;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Bot.Tests.Clockify.Dialogs
{
    public class ClockifySetupDialogTest
    {
        private const string ClockifyApiKey = nameof(ClockifyApiKey);
        private const string SetupRequest = "SetupRequest";
        private const string SetupReject = "SetupReject";
        private const string SetupFeedback = "SetupFeedback";

        private readonly IMiddleware[] _middlewares;

        public ClockifySetupDialogTest(ITestOutputHelper output)
        {
            _middlewares = new IMiddleware[]{ new XUnitDialogTestLogger(output)};
        }

        [Fact]
        private async void ClockifySetupDialog_UserProvideValidToken_GiveFeedbackToUserAndSaveTokenId()
        {
            var userState = new UserState(new MemoryStorage());
            var mockClockifyService = new Mock<IClockifyService>();
            mockClockifyService.Setup(c => c.GetCurrentUserAsync(It.IsAny<string>()))
                .ReturnsAsync(new UserDo
                {
                    Id = "1234",
                    Name = "John Doe",
                    Email = "johndoe@gmail.com",
                    ActiveWorkspace = "w1",
                    DefaultWorkspace = "w1"
                });
            var clockifyMessageSource = new Mock<IClockifyMessageSource>();
            clockifyMessageSource.Setup(c => c.SetupRequest).Returns(SetupRequest);
            clockifyMessageSource.Setup(c => c.SetupFeedback).Returns(SetupFeedback);
            var mockTokenRepository = new Mock<ITokenRepository>();
            mockTokenRepository.Setup(r => r.WriteAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new TokenData("id", ClockifyApiKey));

            var dialog = new ClockifySetupDialog(userState, mockClockifyService.Object, mockTokenRepository.Object,
                clockifyMessageSource.Object);
            var dialogTestClient = new DialogTestClient(Channels.Telegram, dialog, middlewares: _middlewares);

            var reply = await dialogTestClient.SendActivityAsync<IMessageActivity>("ciao");
            reply.Text.Should().Be(SetupRequest);

            reply = await dialogTestClient.SendActivityAsync<IMessageActivity>(ClockifyApiKey);
            reply.Text.Should().Be(SetupFeedback);

            var userProfile = await userState.CreateProperty<UserProfile>("UserProfile")
                .GetAsync(dialogTestClient.DialogContext.Context);

            userProfile.ClockifyTokenId.Should().NotBeNull();
            dialogTestClient.DialogTurnResult.Status.Should().Be(DialogTurnStatus.Complete);
        }

        [Fact]
        private async void ClockifySetupDialog_ClockifyThrows401_ShouldAskAgain()
        {
            var userState = new UserState(new MemoryStorage());
            var clockifyService = new Mock<IClockifyService>();
            clockifyService
                .Setup(service => service.GetCurrentUserAsync(It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedAccessException());
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