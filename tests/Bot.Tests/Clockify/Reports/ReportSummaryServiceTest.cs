using System;
using System.Collections.Generic;
using Bot.Clockify.Client;
using Bot.Clockify.Models;
using Bot.Clockify.Reports;
using Bot.Data;
using Bot.States;
using FluentAssertions;
using Microsoft.Bot.Connector;
using Moq;
using Xunit;

namespace Bot.Tests.Clockify.Reports
{
    public class ReportSummaryServiceTest
    {
        [Fact]
        [UseCulture("en-US")]
        public async void Summary_UserHasTwoWorkspaces_ReturnsSummaryAggregatedByWorkspace()
        {
            // Stub
            var mockClockifyService = new Mock<IClockifyService>();
            var mockTokenRepository = new Mock<ITokenRepository>();

            mockTokenRepository.Setup(r => r.ReadAsync(It.IsAny<string>()))
                .ReturnsAsync(new TokenData("id", "clockifyToken"));
            var workspaces = new List<WorkspaceDo>
            {
                Workspace1(),
                Workspace2()
            };
            mockClockifyService.Setup(c => c.GetWorkspacesAsync(It.IsAny<string>()))
                .ReturnsAsync(workspaces);

            var timeEntriesWorkspace1 = TimeEntriesWorkspace1();
            var timeEntriesWorkspace2 = TimeEntriesWorkspace2();

            mockClockifyService
                .SetupSequence(c => c.GetHydratedTimeEntriesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset>()
                ))
                .ReturnsAsync(timeEntriesWorkspace1)
                .ReturnsAsync(timeEntriesWorkspace2);

            // Arrange
            var userProfile = UserProfile();
            var dateRange = new DateRange(
                new DateTime(2020, 1, 1, 0, 0, 0),
                new DateTime(2020, 3, 1, 0, 0, 0)
            );
            var reportSummaryService = new ReportSummaryService(mockClockifyService.Object, mockTokenRepository.Object,
                TestClockifyUtils.ClockifyMessageSource());

            // Act
            string summary = await reportSummaryService.Summary(Channels.Telegram, userProfile, dateRange);

            // Assert
            summary.Should().BeEquivalentTo($"Between 01 January 2020 - 01 March 2020, you worked **2.72d** (21) hours" +
                                            "\n\n\n\nWork reported on workspace **workspace1**:" +
                                            "\n- **forecasting**: 0.75d" +
                                            "\n- **r&d** - blockchain: 1.53d" +
                                            "\n\nWork reported on workspace **workspace2**:" +
                                            "\n- **operations** - management: 0.44d"
            );
        }

        [Fact]
        [UseCulture("en-US")]
        public async void Summary_UserHasOneWorkspace_DoesNotMentionWorkspaces()
        {
            // Stub
            var mockClockifyService = new Mock<IClockifyService>();
            var mockTokenRepository = new Mock<ITokenRepository>();

            mockTokenRepository.Setup(r => r.ReadAsync(It.IsAny<string>()))
                .ReturnsAsync(new TokenData("id", "clockifyToken"));
            var workspaces = new List<WorkspaceDo>
            {
                Workspace1(),
            };
            mockClockifyService.Setup(c => c.GetWorkspacesAsync(It.IsAny<string>()))
                .ReturnsAsync(workspaces);

            mockClockifyService
                .SetupSequence(c => c.GetHydratedTimeEntriesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset>()
                ))
                .ReturnsAsync(TimeEntriesWorkspace1());

            // Arrange
            var userProfile = UserProfile();
            var dateRange = new DateRange(
                new DateTime(2020, 1, 1, 0, 0, 0),
                new DateTime(2020, 3, 1, 0, 0, 0)
            );
            var reportSummaryService = new ReportSummaryService(mockClockifyService.Object, mockTokenRepository.Object,
                TestClockifyUtils.ClockifyMessageSource());

            // Act
            string summary = await reportSummaryService.Summary(Channels.Telegram, userProfile, dateRange);

            // Assert
            summary.Should().BeEquivalentTo($"Between 01 January 2020 - 01 March 2020, you worked **2.28d** (18) hours" +
                                            "\n\n\n- **forecasting**: 0.75d" +
                                            "\n- **r&d** - blockchain: 1.53d");
        }

        [Fact]
        [UseCulture("en-US")]
        public async void Summary_UserHasNoEntries_DoesntReturnResults()
        {
            // Stub
            var mockClockifyService = new Mock<IClockifyService>();
            var mockTokenRepository = new Mock<ITokenRepository>();

            mockTokenRepository.Setup(r => r.ReadAsync(It.IsAny<string>()))
                .ReturnsAsync(new TokenData("id", "clockifyToken"));
            var workspaces = new List<WorkspaceDo>
            {
                Workspace1()
            };
            mockClockifyService.Setup(c => c.GetWorkspacesAsync(It.IsAny<string>()))
                .ReturnsAsync(workspaces);

            var emptyTimeEntries = new List<HydratedTimeEntryDo>();

            mockClockifyService
                .SetupSequence(c => c.GetHydratedTimeEntriesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset>()
                ))
                .ReturnsAsync(emptyTimeEntries);

            // Arrange
            var userProfile = UserProfile();
            var dateRange = new DateRange(
                new DateTime(2020, 1, 1, 0, 0, 0),
                new DateTime(2020, 3, 1, 0, 0, 0)
            );
            var reportSummaryService = new ReportSummaryService(mockClockifyService.Object, mockTokenRepository.Object,
                TestClockifyUtils.ClockifyMessageSource());

            // Act
            string summary = await reportSummaryService.Summary(Channels.Telegram, userProfile, dateRange);
            summary.Should().BeEquivalentTo("\n\nNo work to report on workspace **workspace1**\n\n");
        }

        private static UserProfile UserProfile()
        {
            return new UserProfile
            {
                ClockifyTokenId = "tokenId",
                UserId = "uid"
            };
        }

        private static WorkspaceDo Workspace1()
        {
            return new WorkspaceDo("id1", "workspace1");
        }

        private static WorkspaceDo Workspace2()
        {
            return new WorkspaceDo("id2", "workspace2");
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

        private static List<HydratedTimeEntryDo> TimeEntriesWorkspace1()
        {
            return new List<HydratedTimeEntryDo>
            {
                new HydratedTimeEntryDo
                (
                    "e1",
                    ProjectRd(),
                    new TimeInterval
                    {
                        Start = new DateTimeOffset(2020, 2, 2, 7, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 2, 2, 15, 0, 0, TimeSpan.Zero),
                    },
                    TaskBlockchain()
                ),
                new HydratedTimeEntryDo
                (
                    "e2",
                    ProjectRd(),
                    new TimeInterval
                    {
                        Start = new DateTimeOffset(2020, 2, 4, 7, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 2, 4, 11, 15, 0, TimeSpan.Zero),
                    },
                    TaskBlockchain()
                ),
                new HydratedTimeEntryDo
                (
                    "e3",
                    ProjectForecasting(),
                    new TimeInterval
                    {
                        Start = new DateTimeOffset(2020, 2, 3, 7, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 2, 3, 13, 0, 0, TimeSpan.Zero),
                    }
                )
            };
        }

        private static List<HydratedTimeEntryDo> TimeEntriesWorkspace2()
        {
            return new List<HydratedTimeEntryDo>
            {
                new HydratedTimeEntryDo
                (
                    "e4",
                    new ProjectDo { Name = "operations" },
                    new TimeInterval
                    {
                        Start = new DateTimeOffset(2020, 2, 3, 15, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 2, 3, 18, 30, 0, TimeSpan.Zero),
                    },
                    new TaskDo { Name = "management" }
                )
            };
        }
    }
}