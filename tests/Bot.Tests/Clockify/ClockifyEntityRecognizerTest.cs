using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Clockify.Fill;
using Bot.Clockify.Models;
using Clockify.Net.Models.Workspaces;
using F23.StringSimilarity;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bot.Tests.Clockify
{
    public class ClockifyEntityRecognizerTest
    {
        [Fact]
        public async void RecognizeProject_NoProjectsFound_ThrowsCannotRecognizeProjectException()
        {
            var input = "sales forecast";

            var clockifyServiceMock = new Mock<IClockifyService>();
            clockifyServiceMock.Setup(c => c.GetWorkspacesAsync(It.IsAny<string>()))
                .ReturnsAsync(Workspaces);
            clockifyServiceMock.Setup(c => c.GetProjectsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ProjectDo>());

            var distanceAlgorithm = new ClockifyEntityDistance(new MetricLCS());
            var projectRecognizer = new ClockifyEntityRecognizer(distanceAlgorithm, clockifyServiceMock.Object);

            Func<Task> action = () =>
                projectRecognizer.RecognizeProject(input, "ignored");

            await action.Should().ThrowAsync<CannotRecognizeProjectException>();
        }

        [Fact]
        public async void RecognizeProject_OneRecognizableProjectFound_ReturnsRecognizedProject()
        {
            var recognizableProject = new ProjectDo {Name = "abec_bundle_sales_forecast"};
            var input = "sales forecast";
            var expected = recognizableProject.Name;

            var clockifyServiceMock = new Mock<IClockifyService>();
            clockifyServiceMock.Setup(c => c.GetWorkspacesAsync(It.IsAny<string>()))
                .ReturnsAsync(Workspaces);
            clockifyServiceMock.Setup(c => c.GetProjectsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ProjectDo> {recognizableProject});
            var distanceAlgorithm = new ClockifyEntityDistance(new MetricLCS());

            var result = await new ClockifyEntityRecognizer(distanceAlgorithm, clockifyServiceMock.Object)
                .RecognizeProject(input, "ignored");

            result.Name.Should().Be(expected);
        }

        [Theory]
        [InlineData("sales forecast", "abec_bundle_sales_forecast")]
        [InlineData("exactMatch", "exactMatch")]
        [InlineData("middle", "xkfc_middle_fo")]
        [InlineData("right", "abcdf_right")]
        public async void RecognizeProject_ExactAndDistanceMatches_ReturnsRecognizedProject(string input,
            string expected)
        {
            var clockifyServiceMock = new Mock<IClockifyService>();
            clockifyServiceMock.Setup(c => c.GetWorkspacesAsync(It.IsAny<string>()))
                .ReturnsAsync(Workspaces);
            clockifyServiceMock.Setup(c => c.GetProjectsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Projects);
            var distanceAlgorithm = new ClockifyEntityDistance(new MetricLCS());

            var result = await new ClockifyEntityRecognizer(distanceAlgorithm, clockifyServiceMock.Object)
                .RecognizeProject(input, "ignored");

            result.Name.Should().Be(expected);
        }

        [Theory]
        [InlineData("bun_")]
        [InlineData("chaos fo")]
        [InlineData("meow")]
        public async Task RecognizeProject_EntitiesWithLowScore_ThrowsCannotRecognizeProjectException(string input)
        {
            var distanceAlgorithm = new ClockifyEntityDistance(new MetricLCS());
            var clockifyServiceMock = new Mock<IClockifyService>();
            clockifyServiceMock.Setup(c => c.GetWorkspacesAsync(It.IsAny<string>()))
                .ReturnsAsync(Workspaces);
            clockifyServiceMock.Setup(c => c.GetProjectsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Projects);

            var projectRecognizer = new ClockifyEntityRecognizer(distanceAlgorithm, clockifyServiceMock.Object);

            Func<Task> action = () =>
                projectRecognizer.RecognizeProject(input, "ignored");

            await action.Should().ThrowAsync<CannotRecognizeProjectException>();
        }

        [Fact]
        public async void RecognizeProject_MoreThenOneTopScore_ThrowsAmbiguousRecognizableProjectException()
        {
            var input = "abcdf";
            var ambiguousProjects = new List<ProjectDo>
            {
                new ProjectDo {Name = "abcdf_right"},
                new ProjectDo {Name = "abcdf_left"},
            };

            var distanceAlgorithm = new ClockifyEntityDistance(new MetricLCS());

            var clockifyServiceMock = new Mock<IClockifyService>();
            clockifyServiceMock.Setup(c => c.GetWorkspacesAsync(It.IsAny<string>()))
                .ReturnsAsync(Workspaces);
            clockifyServiceMock.Setup(c => c.GetProjectsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(ambiguousProjects);

            var projectRecognizer = new ClockifyEntityRecognizer(distanceAlgorithm, clockifyServiceMock.Object);

            Func<Task> action = () =>
                projectRecognizer.RecognizeProject(input, "ignored");

            await action.Should().ThrowAsync<AmbiguousRecognizableProjectException>();
        }

        private static List<WorkspaceDto> Workspaces()
        {
            return new List<WorkspaceDto> {new WorkspaceDto {Id = "id1", Name = "workspace1"}};
        }

        private static List<ProjectDo> Projects()
        {
            return new List<ProjectDo>
            {
                new ProjectDo {Name = "abec_bundle_sales_forecast"},
                new ProjectDo {Name = "xkfc_middle_fo"},
                new ProjectDo {Name = "abcdf_right"},
                new ProjectDo {Name = "exactMatch"}
            };
        }
    }
}