using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Exceptions;
using Bot.Recognizers;
using Bot.Services;
using Bot.Services.Clockify;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.Workspaces;
using F23.StringSimilarity;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bot.Tests.Services.Clockify
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
                .ReturnsAsync(new List<ProjectDtoImpl>());

            var distanceAlgorithm = new ClockifyEntityDistance(new MetricLCS());
            var projectRecognizer = new ClockifyEntityRecognizer(distanceAlgorithm, clockifyServiceMock.Object);

            Func<Task> action = () =>
                projectRecognizer.RecognizeProject(input, "ignored");

            await action.Should().ThrowAsync<CannotRecognizeProjectException>();
        }

        [Fact]
        public async void RecognizeProject_OneRecognizableProjectFound_ReturnsRecognizedProject()
        {
            var recognizableProject = new ProjectDtoImpl {Name = "abec_bundle_sales_forecast"};
            var input = "sales forecast";
            var expected = recognizableProject.Name;

            var clockifyServiceMock = new Mock<IClockifyService>();
            clockifyServiceMock.Setup(c => c.GetWorkspacesAsync(It.IsAny<string>()))
                .ReturnsAsync(Workspaces);
            clockifyServiceMock.Setup(c => c.GetProjectsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<ProjectDtoImpl> {recognizableProject});
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
            var ambiguousProjects = new List<ProjectDtoImpl>
            {
                new ProjectDtoImpl {Name = "abcdf_right"},
                new ProjectDtoImpl {Name = "abcdf_left"},
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

        private static List<ProjectDtoImpl> Projects()
        {
            return new List<ProjectDtoImpl>
            {
                new ProjectDtoImpl {Name = "abec_bundle_sales_forecast"},
                new ProjectDtoImpl {Name = "xkfc_middle_fo"},
                new ProjectDtoImpl {Name = "abcdf_right"},
                new ProjectDtoImpl {Name = "exactMatch"}
            };
        }
    }
}