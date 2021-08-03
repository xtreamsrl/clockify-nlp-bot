using System;
using Bot.Clockify;
using FluentAssertions;
using Xunit;

namespace Bot.Tests.Clockify.Reports
{
    public class DateRangeTest
    {
        [Fact]
        public void FillDateRange_DateRangeEndIsNull_FillDateRangeEndWithToday()
        {
            var filledDateRange = new DateRange(new DateTime(2020, 1, 1, 0, 0, 0), null);

            filledDateRange.End.Should().Be(DateTime.Today);
        }

        [Fact]
        public void FillDateRange_DateRangeStartIsNull_FillDateRangeStartWithUnixEpoch()
        {
            var filledDateRange = new DateRange(null, DateTime.Today);

            filledDateRange.Start.Should().HaveYear(1970);
        }

        [Fact]
        [UseCulture("it-IT")]
        public void ToStringTest_Italian()
        {
            var expected = DateRange.FromString("10/07/2020 00:10:00", "23/08/2020");
            expected.ToString().Should().Be("10 luglio 2020 - 23 agosto 2020");
        }

        [Fact]
        [UseCulture("en-US")]
        public void ToStringTest_UnitedStates()
        {
            var expected = DateRange.FromString("07/10/2020 00:10:00", "08/23/2020");
            expected.ToString().Should().Be("10 July 2020 - 23 August 2020");
        }
    }
}