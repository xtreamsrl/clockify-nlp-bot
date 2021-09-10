using System;
using Bot.Clockify;
using Bot.Clockify.Reports;
using Bot.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bot.Tests.Clockify.Reports
{
    public class ReportExtractorTest
    {

        [Fact]
        public void GetDateRangeFromTimePeriod_ValidTimePeriod_ShouldReturnDateRange()
        {
            const string timePeriod = "from 01 July to 10 July";
            var today = new DateTime(2020, 08, 01, 0, 0, 0);
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(p => p.DateTimeNow()).Returns(today);
            var reportExtractor = new ReportExtractor(dateTimeProviderMock.Object);

            var dateRange = reportExtractor.GetDateRangeFromTimePeriod(timePeriod);

            var expectedStart = new DateTime(2020, 07, 01, 0, 0, 0);
            var expectedEnd = new DateTime(2020, 07, 10, 0, 0, 0);
            
            dateRange.Start.Should().Be(expectedStart);
            dateRange.End.Should().Be(expectedEnd);
        }
        
        [Fact]
        public void GetDateRangeFromTimePeriod_InvalidTimePeriod_ThrowsException()
        {
            const string timePeriod = "invalid time period";

            var reportExtractor = new ReportExtractor(new Mock<IDateTimeProvider>().Object);

            Func<DateRange> action = () => reportExtractor.GetDateRangeFromTimePeriod(timePeriod);

            action.Should().ThrowExactly<InvalidDateRangeException>().WithMessage(
                "Cannot parse invalid time period date range.");
        }
    }
}