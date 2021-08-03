using System;
using Bot.Remind;
using Bot.States;
using FluentAssertions;
using Xunit;

namespace Bot.Tests.Remind
{
    public class UserDidNotSayStopTest
    {
        [Fact]
        public async void UserDidNotSayStop_StopRemindIsTodayUTC_ReturnFalse()
        {
            var userSaidStopAlready = new UserProfile
            {
                StopRemind = DateTime.Today.ToUniversalTime()
            };

            var userDidNotSayStop = new UserDidNotSayStop();
            bool reminderIsNeeded = await userDidNotSayStop.ReminderIsNeeded(userSaidStopAlready);

            reminderIsNeeded.Should().BeFalse();
        }
        
        [Fact]
        public async void UserDidNotSayStop_StopRemindIsNotTodayUTC_ReturnTrue()
        {
            var pastDate = new DateTime(2020, 7, 1);
            var userDidntSayStopToday = new UserProfile
            {
                StopRemind = pastDate.ToUniversalTime()
            };

            var userDidNotSayStop = new UserDidNotSayStop();
            bool reminderIsNeeded = await userDidNotSayStop.ReminderIsNeeded(userDidntSayStopToday);

            reminderIsNeeded.Should().BeTrue();
        }
        
        [Fact]
        public async void UserDidNotSayStop_StopRemindIsNull_ReturnTrue()
        {
            var userNeverSaidStop = new UserProfile();

            var userDidNotSayStop = new UserDidNotSayStop();
            bool reminderIsNeeded = await userDidNotSayStop.ReminderIsNeeded(userNeverSaidStop);

            reminderIsNeeded.Should().BeTrue();
        }
    }
}