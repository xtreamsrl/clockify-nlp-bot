using Bot.Clockify;
using Bot.Clockify.Client;
using Bot.Dialogs;
using Bot.Services;
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
            var clockifyService = new ClockifyService();

            var dialog = new ClockifySetupDialog(userState, clockifyService);
            var dialogTestClient = new DialogTestClient(Channels.Telegram, dialog);

            var reply = await dialogTestClient.SendActivityAsync<IMessageActivity>("ciao");
            reply.Text.Should().Be(ClockifySetupDialog.Request);

            reply = await dialogTestClient.SendActivityAsync<IMessageActivity>(InvalidApiKey);
            reply.Text.Should().Be(ClockifySetupDialog.Reject);

            reply = await dialogTestClient.SendActivityAsync<IMessageActivity>(ClockifyApiKey);
            reply.Text.Should().Be(ClockifySetupDialog.Feedback);

            var userProfile = await userState.CreateProperty<UserProfile>("UserProfile")
                .GetAsync(dialogTestClient.DialogContext.Context);

            userProfile.ClockifyToken.Should().Be(ClockifyApiKey);
            userProfile.UserId.Should().NotBeNull().And.HaveLength(24);

            // TODO: How do we test the end of the dialog?
        }

        [Fact]
        private async void UserSetupDialog_ClockifyIsDown_ShouldReturnMessageError()
        {
            var userState = new UserState(new MemoryStorage());
            var clockifyService = new Mock<IClockifyService>();
            clockifyService
                .Setup(service => service.GetCurrentUserAsync(It.IsAny<string>()))
                .ThrowsAsync(new ErrorResponseException(ClockifySetupDialog.Reject));

            var dialog = new ClockifySetupDialog(userState, clockifyService.Object);
            var dialogTestClient = new DialogTestClient(Channels.Telegram, dialog);

            var reply = await dialogTestClient.SendActivityAsync<IMessageActivity>("ciao");
            reply.Text.Should().Be(ClockifySetupDialog.Request);

            reply = await dialogTestClient.SendActivityAsync<IMessageActivity>(ClockifyApiKey);

            reply.Text.Should().Be(ClockifySetupDialog.Reject);
        }
    }
}