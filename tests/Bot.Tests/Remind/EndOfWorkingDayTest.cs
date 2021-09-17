using System;
using Bot.Common;
using Bot.Remind;
using Bot.States;
using FluentAssertions;
using Moq;
using TimeZoneConverter;
using Xunit;

namespace Bot.Tests.Remind
{
    public class EndOfWorkingDayTest
    {
        
        [Fact]
        public void ReminderIsNeeded_TimeToRemindLosAngelesUser_ReturnsTrue()
        {
            // Los angeles has a -8 or -7 ut offset
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(d => d.DateTimeUtcNow())
                .Returns(new DateTime(2021, 9, 15, 4, 0, 0));
            
            var needRemindService = new EndOfWorkingDay(dateTimeProviderMock.Object);
            
            var userProfile = new UserProfile
            {
                TimeZone = TZConvert.GetTimeZoneInfo("America/Los_Angeles")
            };

            needRemindService.ReminderIsNeeded(userProfile).Result.Should().Be(true);
        }
        
        [Fact]
        public void ReminderIsNeeded_TimeToNotRemindLosAngelesUser_ReturnsFalse()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(d => d.DateTimeUtcNow())
                .Returns(new DateTime(2021, 9, 15, 18, 0, 0));
            
            var needRemindService = new EndOfWorkingDay(dateTimeProviderMock.Object);
            
            var userProfile = new UserProfile
            {
                TimeZone = TZConvert.GetTimeZoneInfo("America/Los_Angeles")
            };

            needRemindService.ReminderIsNeeded(userProfile).Result.Should().Be(false);
        }
        
        [Fact]
        public void ReminderIsNeeded_SundayInLosAngeles_ReturnsFalse()
        {
            var mondayUtc = new DateTime(2021, 9, 20, 4, 0, 0);
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(d => d.DateTimeUtcNow())
                .Returns(mondayUtc);
            
            var needRemindService = new EndOfWorkingDay(dateTimeProviderMock.Object);
            
            var userProfile = new UserProfile
            {
                TimeZone = TZConvert.GetTimeZoneInfo("America/Los_Angeles")
            };

            needRemindService.ReminderIsNeeded(userProfile).Result.Should().Be(false);
        }
    }
}