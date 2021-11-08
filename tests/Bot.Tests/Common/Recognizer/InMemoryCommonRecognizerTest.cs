using System;
using System.Threading.Tasks;
using Bot.Common.Recognizer;
using FluentAssertions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Schema;
using Xunit;

namespace Bot.Tests.Common.Recognizer
{
    public class InMemoryCommonRecognizerTest
    {
        [Fact]
        private async void GenericRecognizeAsync_FillMessage_ExtractsIntentAndEntities()
        {
            const string intent = "fill";
            const string duration = "2 hours";
            const string project = "brand";
            var messageActivity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = $"{intent}: {duration}, {project}"
            };
            var turnContext = new TurnContext(new TestAdapter(), messageActivity);

            var result = await new InMemoryCommonRecognizer().RecognizeAsync<TimeSurveyBotLuis>(turnContext, default);

            result.Intents.Count.Should().Be(1);
            result.Intents.ContainsKey(TimeSurveyBotLuis.Intent.Fill).Should().BeTrue();
            result.Entities._instance.datetime.Should().HaveCount(1);
            result.Entities._instance.datetime[0].Text.Should().Be(duration);
            result.Entities._instance.datetime[0].Type.Should().Be("builtin.datetimeV2.duration");
            result.Entities._instance.WorkedEntity.Should().HaveCount(1);
            result.Entities._instance.WorkedEntity[0].Text.Should().Be(project);
        }

        [Fact]
        private async void GenericRecognizeAsync_ReportMessage_ExtractsIntentAndEntities()
        {
            const string intent = "report";
            const string daterange = "this week";
            var messageActivity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = $"{intent}: {daterange}"
            };
            var turnContext = new TurnContext(new TestAdapter(), messageActivity);

            var result = await new InMemoryCommonRecognizer().RecognizeAsync<TimeSurveyBotLuis>(turnContext, default);

            result.Intents.Count.Should().Be(1);
            result.Intents.ContainsKey(TimeSurveyBotLuis.Intent.Report).Should().BeTrue();
            result.Entities._instance.datetime.Should().HaveCount(1);
            result.Entities._instance.datetime[0].Text.Should().Be("this week");
            result.Entities._instance.datetime[0].Type.Should().Be("builtin.datetimeV2.daterange");
        }

        [Fact]
        private async void GenericRecognizeAsync_NotMessageActivity_ReturnsDefault()
        {
            var eventActivity = new Activity
            {
                Type = ActivityTypes.Event
            };
            var turnContext = new TurnContext(new TestAdapter(), eventActivity);

            var result = await new InMemoryCommonRecognizer().RecognizeAsync<TimeSurveyBotLuis>(turnContext, default);

            result.Intents.Count.Should().Be(1);
            result.Intents.ContainsKey(TimeSurveyBotLuis.Intent.None).Should().BeTrue();
        }

        [Fact]
        private async void GenericRecognizeAsync_EmptyUtterance_ReturnsDefault()
        {
            var messageActivity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = ""
            };
            var turnContext = new TurnContext(new TestAdapter(), messageActivity);

            var result = await new InMemoryCommonRecognizer().RecognizeAsync<TimeSurveyBotLuis>(turnContext, default);

            result.Intents.Count.Should().Be(1);
            result.Intents.ContainsKey(TimeSurveyBotLuis.Intent.None).Should().BeTrue();
        }

        [Fact]
        private async void GenericRecognizeAsync_UnrecognizedIntent_ReturnsDefault()
        {
            var messageActivity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = "invalid intent"
            };
            var turnContext = new TurnContext(new TestAdapter(), messageActivity);

            var result = await new InMemoryCommonRecognizer().RecognizeAsync<TimeSurveyBotLuis>(turnContext, default);

            result.Intents.Count.Should().Be(1);
            result.Intents.ContainsKey(TimeSurveyBotLuis.Intent.None).Should().BeTrue();
        }
        
        [Fact]
        private async void GenericRecognizeAsync_OneFillEntity_ThrowsArgumentException()
        {
            const string intent = "fill";
            const string duration = "2 hours";
            var messageActivity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = $"{intent}: {duration}, "
            };
            var turnContext = new TurnContext(new TestAdapter(), messageActivity);

            Func<Task> action = () =>
                new InMemoryCommonRecognizer().RecognizeAsync<TimeSurveyBotLuis>(turnContext, default);

            await action.Should().ThrowExactlyAsync<ArgumentException>();
        }

        [Fact]
        private async void GenericRecognizeAsync_NoFillDurationEntity_ThrowsArgumentException()
        {
            const string intent = "fill";
            const string duration = "04 feb 2020";
            const string project = "brand";
            var messageActivity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = $"{intent}: {duration}, {project}"
            };

            var turnContext = new TurnContext(new TestAdapter(), messageActivity);
            Func<Task> action = () =>
                new InMemoryCommonRecognizer().RecognizeAsync<TimeSurveyBotLuis>(turnContext, default);

            await action.Should().ThrowExactlyAsync<ArgumentException>();
        }

        [Fact]
        private async void GenericRecognizeAsync_MultipleReportEntities_ThrowsArgumentException()
        {
            const string intent = "report";
            const string daterange = "this week";
            var messageActivity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = $"{intent}: {daterange}, {daterange}"
            };
            var turnContext = new TurnContext(new TestAdapter(), messageActivity);

            Func<Task> action = () =>
                new InMemoryCommonRecognizer().RecognizeAsync<TimeSurveyBotLuis>(turnContext, default);

            await action.Should().ThrowExactlyAsync<ArgumentException>();
        }

        [Fact]
        private async void GenericRecognizeAsync_InvalidReportDateRange_ThrowsArgumentException()
        {
            const string intent = "report";
            const string duration = "8 hours";
            var messageActivity = new Activity
            {
                Type = ActivityTypes.Message,
                Text = $"{intent}: {duration}"
            };
            var turnContext = new TurnContext(new TestAdapter(), messageActivity);

            Func<Task> action = () =>
                new InMemoryCommonRecognizer().RecognizeAsync<TimeSurveyBotLuis>(turnContext, default);

            await action.Should().ThrowExactlyAsync<ArgumentException>();
        }
    }
}