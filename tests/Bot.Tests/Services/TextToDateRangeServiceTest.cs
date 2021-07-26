using System;
using Bot.Models;
using Bot.Utils;
using FluentAssertions;
using Xunit;

namespace Bot.Tests.Services
{
    public class TextToDateRangeServiceTest
    {
        [Fact]
        public void DateRange_FromTo()
        {
            var dateRange = TextToDateRangeService.Convert("from 4 days ago to now");
            var expected = new DateRange(DateTime.Now.AddDays(-4).Date, DateTime.Now.Date);
            dateRange.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void DateRange_Since()
        {
            var dateRange = TextToDateRangeService.Convert("since the beginning of the month");
            var expected = new DateRange(DateTime.Now.AddDays(1 - DateTime.Now.Day).Date, DateTime.Today);
            dateRange.Should().BeEquivalentTo(expected);
        }

        [Fact]
        // the end is inclusive
        public void DateRange_Until()
        {
            var dateRange = TextToDateRangeService.Convert("until two days ago");
            var expected = new DateRange(DateTime.UnixEpoch, DateTime.Now.AddDays(-2).Date);
            dateRange.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void InvalidDateAndDateRange()
        {
            var dateRange = TextToDateRangeService.Convert("invalid text with no date nor date range");
            dateRange.Should().BeNull();
        }

        [Fact]
        public void Time()
        {
            var dateRange = TextToDateRangeService.Convert("5pm");
            dateRange.Should().BeNull();
        }

        [Fact]
        public void Today_ShouldBeOneDayLong()
        {
            TextToDateRangeService.Convert("today")?.Length().Days.Should().Be(1);
        }

        [Fact]
        public void AWeek_ShouldBe7DaysLong()
        {
            TextToDateRangeService.Convert("this week")?.Length().Days.Should().Be(7);
            TextToDateRangeService.Convert("last week")?.Length().Days.Should().Be(7);
        }

        [Fact]
        public void AMonth_ShouldLastProperly()
        {
            int expected = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            TextToDateRangeService.Convert("this month")?.Length().Days.Should().Be(expected);
        }
    }
}