using System;
using System.Collections.Generic;
using Bot.Models;
using Bot.Services;
using Bot.Services.Reports;
using Bot.States;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Tasks;
using Clockify.Net.Models.TimeEntries;
using Clockify.Net.Models.Workspaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bot.Tests.Services.Reports
{
    public class ReportSummaryServiceTest
    {
        [Fact]
        [UseCulture("en-US")]
        public async void Summary_UserHasTwoWorkspaces_ReturnsSummaryAggregatedByWorkspace()
        {
            // Stub
            var mockClockifyService = new Mock<IClockifyService>();

            var workspaces = new List<WorkspaceDto>
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
            var reportSummaryService = new ReportSummaryService(mockClockifyService.Object);

            // Act
            string summary = await reportSummaryService.Summary(userProfile, dateRange);

            // Assert
            summary.Should().BeEquivalentTo($"You worked **2.72d** in 01 January 2020 - 01 March 2020" +
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

            var workspaces = new List<WorkspaceDto>
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
            var reportSummaryService = new ReportSummaryService(mockClockifyService.Object);

            // Act
            string summary = await reportSummaryService.Summary(userProfile, dateRange);

            // Assert
            summary.Should().BeEquivalentTo($"You worked **2.28d** in 01 January 2020 - 01 March 2020" +
                                            "\n\n\n- **forecasting**: 0.75d" +
                                            "\n- **r&d** - blockchain: 1.53d");
        }

        [Fact]
        [UseCulture("en-US")]
        public async void Summary_UserHasNoEntries_DoesntReturnResults()
        {
            // Stub
            var mockClockifyService = new Mock<IClockifyService>();

            var workspaces = new List<WorkspaceDto>
            {
                Workspace1()
            };
            mockClockifyService.Setup(c => c.GetWorkspacesAsync(It.IsAny<string>()))
                .ReturnsAsync(workspaces);

            var emptyTimeEntries = new List<HydratedTimeEntryDtoImpl>();

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
            var reportSummaryService = new ReportSummaryService(mockClockifyService.Object);

            // Act
            string summary = await reportSummaryService.Summary(userProfile, dateRange);
            summary.Should().BeEquivalentTo("\n\nNo work to report on workspace **workspace1**\n\n");
        }

        private static UserProfile UserProfile()
        {
            return new UserProfile
            {
                ClockifyToken = "token",
                UserId = "uid"
            };
        }

        private static WorkspaceDto Workspace1()
        {
            return new WorkspaceDto {Id = "id1", Name = "workspace1"};
        }

        private static WorkspaceDto Workspace2()
        {
            return new WorkspaceDto {Id = "id2", Name = "workspace2"};
        }


        private static ProjectDtoImpl ProjectRd()
        {
            return new ProjectDtoImpl {Name = "r&d"};
        }

        private static ProjectDtoImpl ProjectForecasting()
        {
            return new ProjectDtoImpl {Name = "forecasting"};
        }

        private static TaskDto TaskBlockchain()
        {
            return new TaskDto {Name = "blockchain"};
        }

        private static List<HydratedTimeEntryDtoImpl> TimeEntriesWorkspace1()
        {
            return new List<HydratedTimeEntryDtoImpl>
            {
                new HydratedTimeEntryDtoImpl
                {
                    Project = ProjectRd(),
                    Task = TaskBlockchain(),
                    TimeInterval = new TimeIntervalDto
                    {
                        Start = new DateTimeOffset(2020, 2, 2, 7, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 2, 2, 15, 0, 0, TimeSpan.Zero),
                    }
                },
                new HydratedTimeEntryDtoImpl
                {
                    Project = ProjectRd(),
                    Task = TaskBlockchain(),
                    TimeInterval = new TimeIntervalDto
                    {
                        Start = new DateTimeOffset(2020, 2, 4, 7, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 2, 4, 11, 15, 0, TimeSpan.Zero),
                    }
                },
                new HydratedTimeEntryDtoImpl
                {
                    Project = ProjectForecasting(),
                    TimeInterval = new TimeIntervalDto
                    {
                        Start = new DateTimeOffset(2020, 2, 3, 7, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 2, 3, 13, 0, 0, TimeSpan.Zero),
                    }
                }
            };
        }

        private static List<HydratedTimeEntryDtoImpl> TimeEntriesWorkspace2()
        {
            return new List<HydratedTimeEntryDtoImpl>
            {
                new HydratedTimeEntryDtoImpl
                {
                    Project = new ProjectDtoImpl {Name = "operations"},
                    Task = new TaskDto {Name = "management"},
                    TimeInterval = new TimeIntervalDto
                    {
                        Start = new DateTimeOffset(2020, 2, 3, 15, 0, 0, TimeSpan.Zero),
                        End = new DateTimeOffset(2020, 2, 3, 18, 30, 0, TimeSpan.Zero),
                    }
                }
            };
        }
    }
}