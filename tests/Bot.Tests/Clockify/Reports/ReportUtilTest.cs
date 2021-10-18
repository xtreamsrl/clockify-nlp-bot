﻿using System;
using System.Linq;
using Bot.Clockify.Models;
using Bot.Clockify.Reports;
using FluentAssertions;
using Microsoft.Bot.Connector;
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
                new HydratedTimeEntryDo
                (
                    "e1",
                    ProjectRd(),
                    new TimeInterval
                    {
                        Start = new DateTimeOffset(2020, 7, 6, 7, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 7, 6, 15, 0, 0, TimeSpan.Zero)
                    },
                    TaskBlockchain()
                ),
                new HydratedTimeEntryDo
                (
                    "e2",
                    ProjectRd(),
                    new TimeInterval
                    {
                        Start = new DateTimeOffset(2020, 7, 7, 7, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 7, 7, 14, 15, 0, TimeSpan.Zero)
                    },
                    TaskBlockchain()
                ),
                new HydratedTimeEntryDo
                (
                    "e3",
                    ProjectForecasting(),
                    new TimeInterval
                    {
                        Start = new DateTimeOffset(2020, 7, 8, 7, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 7, 8, 14, 30, 0, TimeSpan.Zero)
                    }
                )
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

            string summary = ReportUtil.SummaryForReportEntries(Channels.Telegram, reportEntries);

            summary.Should().BeEquivalentTo("\n- **forecasting**: 0.94d" +
                                            "\n- **r&d** - blockchain: 1.91d");
        }
        
        [Fact]
        [UseCulture("en-US")]
        public void GenerateSummary_ProjectWithUnderscoreOnTelegramChannel_ReturnsFormattedListOfEntries()
        {
            var reportEntries = new[]
            {
                new ReportEntry("r&d", "blockchain", 15.25f),
                new ReportEntry("brand_identity", "", 7.5f)
            };

            string summary = ReportUtil.SummaryForReportEntries(Channels.Telegram, reportEntries);

            summary.Should().BeEquivalentTo("\n- **brand\\_identity**: 0.94d" +
                                            "\n- **r&d** - blockchain: 1.91d");
        }


        [Fact]
        public void ConvertToReportEntriesWithoutEnd_DurationIsZero()
        {
            var hydratedTimeEntries = new[]
            {
                new HydratedTimeEntryDo
                (
                    "e1",
                    ProjectForecasting(),
                    new TimeInterval
                    {
                        Start = new DateTimeOffset(2020, 7, 8, 7, 0, 0, TimeSpan.Zero),
                    }
                )
            };

            var reportEntries = ReportUtil.ConvertToReportEntries(hydratedTimeEntries);

            reportEntries.First().Hours.Should().Be(0);
        }

        private static ProjectDo ProjectRd()
        {
            return new ProjectDo { Name = "r&d" };
        }

        private static ProjectDo ProjectForecasting()
        {
            return new ProjectDo { Name = "forecasting" };
        }

        private static TaskDo TaskBlockchain()
        {
            return new TaskDo { Name = "blockchain" };
        }
    }
}