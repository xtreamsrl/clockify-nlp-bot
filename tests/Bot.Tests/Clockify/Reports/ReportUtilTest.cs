using System;
using System.Linq;
using Bot.Clockify.Models;
using Bot.Clockify.Reports;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Tasks;
using Clockify.Net.Models.TimeEntries;
using FluentAssertions;
using Xunit;

namespace Bot.Tests.Clockify.Reports
{
    public class ReportUtilTest
    {
        [Fact]
        public void ConvertToReportEntries_ReturnsConvertedEntries()
        {
            var hydratedTimeEntries = new[]
            {
                new HydratedTimeEntryDtoImpl
                {
                    Project = ProjectRd(),
                    Task = TaskBlockchain(),
                    TimeInterval = new TimeIntervalDto
                    {
                        Start = new DateTimeOffset(2020, 7, 6, 7, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 7, 6, 15, 0, 0, TimeSpan.Zero)
                    }
                },
                new HydratedTimeEntryDtoImpl
                {
                    Project = ProjectRd(),
                    Task = TaskBlockchain(),
                    TimeInterval = new TimeIntervalDto
                    {
                        Start = new DateTimeOffset(2020, 7, 7, 7, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 7, 7, 14, 15, 0, TimeSpan.Zero)
                    }
                },
                new HydratedTimeEntryDtoImpl
                {
                    Project = ProjectForecasting(),
                    Task = null,
                    TimeInterval = new TimeIntervalDto
                    {
                        Start = new DateTimeOffset(2020, 7, 8, 7, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 7, 8, 14, 30, 0, TimeSpan.Zero)
                    }
                }
            };

            object[] expectedReportEntries =
            {
                new ReportEntry("r&d", "blockchain", 15.25f),
                new ReportEntry("forecasting", "", 7.5f)
            };

            var reportEntries = ReportUtil.ConvertToReportEntries(hydratedTimeEntries);

            reportEntries.Should().BeEquivalentTo(expectedReportEntries);
        }

        [Fact]
        [UseCulture("en-US")]
        public void GenerateSummary_ReturnsFormattedListOfEntries()
        {
            var reportEntries = new[]
            {
                new ReportEntry("r&d", "blockchain", 15.25f),
                new ReportEntry("forecasting", "", 7.5f)
            };

            var summary = ReportUtil.SummaryForReportEntries(reportEntries);

            summary.Should().BeEquivalentTo("\n- **forecasting**: 0.94d" +
                                            "\n- **r&d** - blockchain: 1.91d");
        }


        [Fact]
        public void ConvertToReportEntriesWithoutEnd_DurationIsZero()
        {
            var hydratedTimeEntries = new[]
            {
                new HydratedTimeEntryDtoImpl
                {
                    Project = ProjectForecasting(),
                    Task = null,
                    TimeInterval = new TimeIntervalDto
                    {
                        Start = new DateTimeOffset(2020, 7, 8, 7, 0, 0, TimeSpan.Zero),
                    }
                }
            };

            var reportEntries = ReportUtil.ConvertToReportEntries(hydratedTimeEntries);

            reportEntries.First().Hours.Should().Be(0);
        }
        
        private static ProjectDo ProjectRd()
        {
            return new ProjectDo {Name = "r&d"};
        }
        
        private static ProjectDo ProjectForecasting()
        {
            return new ProjectDo {Name = "forecasting"};
        }

        private static TaskDto TaskBlockchain()
        {
            return new TaskDto {Name = "blockchain"};
        }

    }
}