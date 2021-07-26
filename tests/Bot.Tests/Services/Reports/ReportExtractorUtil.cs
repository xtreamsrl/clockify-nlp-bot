using System;
using Bot.Exceptions;
using Bot.Models;
using Bot.Services.Reports;
using FluentAssertions;
using Luis;
using Microsoft.Bot.Builder.AI.Luis;
using Moq;
using Xunit;

namespace Bot.Tests.Services.Reports
{
    public class ReportExtractorUtil
    {
        [Fact]
        public void GetDateTimeInstance_ValidEntitiesInstance_ShouldReturnFirstDateTimeText()
        {
            const string timePeriod = "from 01 July to 10 July";
            var entities = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = "from 01 July to 10 July",
                        Type = "builtin.datetimeV2.daterange"
                    },
                    new InstanceData
                    {
                        Text = "this week",
                        Type = "builtin.datetimeV2.daterange"
                    }
                }
            };
            
            var reportExtractor = new ReportExtractor(new Mock<IDateTimeProvider>().Object);

            string timePeriodInstance = reportExtractor.GetDateTimeInstance(entities);
            timePeriodInstance.Should().Be(timePeriod);
        }
        
        [Fact]
        public void GetDateTimeInstance_NullOrEmptyDateTimeInstance_ThrowsException()
        {
            var emptyDateTimeTextEntities = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = new[]
                {
                    new InstanceData
                    {
                        Text = null,
                        Type = null
                    }
                }
            };

            var nullDateTimeEntities = new TimeSurveyBotLuis._Entities._Instance
            {
                datetime = null
            };
            
            var reportExtractor = new ReportExtractor(new Mock<IDateTimeProvider>().Object);

            Func<string> getDateTimeWithNullDateTimeEntities = () =>  reportExtractor.GetDateTimeInstance(nullDateTimeEntities);
            getDateTimeWithNullDateTimeEntities.Should().ThrowExactly<InvalidWorkedPeriodInstanceException>()
                .WithMessage("I can see you want to report some hours, but I really can't understand how many 😕");
            
            Func<string> getDateTimeWithEmptyEntities = () =>  reportExtractor.GetDateTimeInstance(new TimeSurveyBotLuis._Entities._Instance());
            getDateTimeWithEmptyEntities.Should().ThrowExactly<InvalidWorkedPeriodInstanceException>()
                .WithMessage("I can see you want to report some hours, but I really can't understand how many 😕");
            
            Func<string> getDateTimeWithNullDateTimeText = () =>  reportExtractor.GetDateTimeInstance(emptyDateTimeTextEntities);
            getDateTimeWithNullDateTimeText.Should().ThrowExactly<InvalidWorkedPeriodInstanceException>()
                .WithMessage("I can see you want to report some hours, but I really can't understand how many 😕");
        }
        
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
                "I get that you want a report, but I can't understand the period you requested 😕. " +
                "Can you be more specific?");
        }
    }
}