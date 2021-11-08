using System;
using System.Collections.Generic;
using Bot.Clockify;
using Bot.Common;
using Bot.States;
using FluentAssertions;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Bot.Tests.Clockify
{
    public class TestFollowUpService
    {
        [Fact]
        private async void SendFollowUpAsync_ClockifyTokenIsNullWithoutFollowUp_FollowUp()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
            var logger = NullLoggerFactory.Instance.CreateLogger<FollowUpService>();

            var datetimeNow = new DateTime(2021, 5, 5, 16, 0, 0);
            var userWithoutFollowUp = new UserProfile
            {
                LastConversationUpdate = datetimeNow.Subtract(TimeSpan.FromDays(3)),
                ConversationReference = new ConversationReference()
            };
            var expected = new List<UserProfile>
            {
                new UserProfile
                {
                    LastConversationUpdate = datetimeNow.Subtract(TimeSpan.FromDays(3)),
                    ConversationReference = new ConversationReference(),
                    LastFollowUpTimestamp = datetimeNow
                }
            };

            var mockUserProfileProvider = new Mock<IUserProfilesProvider>();
            mockUserProfileProvider.Setup(u => u.GetUserProfilesAsync())
                .ReturnsAsync(new List<UserProfile> { userWithoutFollowUp });
            var mockMessageSource = new Mock<IClockifyMessageSource>();
            mockMessageSource.Setup(m => m.FollowUp).Returns("FollowUpMessage");
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(d => d.DateTimeUtcNow()).Returns(datetimeNow);

            var followUpService = new FollowUpService(mockUserProfileProvider.Object, configuration,
                mockMessageSource.Object, mockDateTimeProvider.Object, logger);

            var adapter = new TestAdapter();

            var followedUsers = await followUpService.SendFollowUpAsync(adapter);

            followedUsers.Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        private async void SendFollowUpAsync_FollowedUpAlready_DoNotFollowUp()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
            var logger = NullLoggerFactory.Instance.CreateLogger<FollowUpService>();

            var datetimeNow = new DateTime(2021, 5, 5, 16, 0, 0);
            var userFollowedUpAlready = new UserProfile
            {
                LastConversationUpdate = datetimeNow.Subtract(TimeSpan.FromDays(3)),
                ConversationReference = new ConversationReference(),
                LastFollowUpTimestamp = DateTime.UtcNow.Subtract(TimeSpan.FromDays(3))
            };

            var mockUserProfileProvider = new Mock<IUserProfilesProvider>();
            mockUserProfileProvider.Setup(u => u.GetUserProfilesAsync())
                .ReturnsAsync(new List<UserProfile> { userFollowedUpAlready });
            var mockMessageSource = new Mock<IClockifyMessageSource>();
            mockMessageSource.Setup(m => m.FollowUp).Returns("FollowUpMessage");
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(d => d.DateTimeUtcNow()).Returns(datetimeNow);

            var followUpService = new FollowUpService(mockUserProfileProvider.Object, configuration,
                mockMessageSource.Object, mockDateTimeProvider.Object, logger);

            var adapter = new TestAdapter();

            var followedUsers = await followUpService.SendFollowUpAsync(adapter);

            followedUsers.Should().BeEmpty();
        }
        
        [Fact]
        private async void SendFollowUpAsync_LastConversationUpdateIsBeforeJanuary2021_DoNotFollowUp()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
            var logger = NullLoggerFactory.Instance.CreateLogger<FollowUpService>();

            var datetimeNow = new DateTime(2021, 5, 5, 16, 0, 0);
            var previousYear = new DateTime(2020, 12, 31, 0, 0, 0);
            var userWithoutClockifyToken = new UserProfile
            {
                LastConversationUpdate = previousYear,
                ConversationReference = new ConversationReference()
            };

            var mockUserProfileProvider = new Mock<IUserProfilesProvider>();
            mockUserProfileProvider.Setup(u => u.GetUserProfilesAsync())
                .ReturnsAsync(new List<UserProfile> { userWithoutClockifyToken });
            var mockMessageSource = new Mock<IClockifyMessageSource>();
            mockMessageSource.Setup(m => m.FollowUp).Returns("FollowUpMessage");
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(d => d.DateTimeUtcNow()).Returns(datetimeNow);

            var followUpService = new FollowUpService(mockUserProfileProvider.Object, configuration,
                mockMessageSource.Object, mockDateTimeProvider.Object, logger);

            var adapter = new TestAdapter();

            var followedUsers = await followUpService.SendFollowUpAsync(adapter);

            followedUsers.Should().BeEmpty();
        }
        
        [Fact]
        private async void SendFollowUpAsync_ClockifyTokenSetAlready_DoNotFollowUp()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
            var logger = NullLoggerFactory.Instance.CreateLogger<FollowUpService>();

            var datetimeNow = new DateTime(2021, 5, 5, 16, 0, 0);
            var userWithClockifyToken = new UserProfile
            {
                ClockifyTokenId = "clockifyToken",
                ConversationReference = new ConversationReference()
            };

            var mockUserProfileProvider = new Mock<IUserProfilesProvider>();
            mockUserProfileProvider.Setup(u => u.GetUserProfilesAsync())
                .ReturnsAsync(new List<UserProfile> { userWithClockifyToken });
            var mockMessageSource = new Mock<IClockifyMessageSource>();
            mockMessageSource.Setup(m => m.FollowUp).Returns("FollowUpMessage");
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockDateTimeProvider.Setup(d => d.DateTimeUtcNow()).Returns(datetimeNow);

            var followUpService = new FollowUpService(mockUserProfileProvider.Object, configuration,
                mockMessageSource.Object, mockDateTimeProvider.Object, logger);

            var adapter = new TestAdapter();

            var followedUsers = await followUpService.SendFollowUpAsync(adapter);

            followedUsers.Should().BeEmpty();
        }
    }
}