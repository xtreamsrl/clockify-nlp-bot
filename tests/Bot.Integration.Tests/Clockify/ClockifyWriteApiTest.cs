using System;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Bot.Clockify.Models;
using Bot.Integration.Tests.Clockify.Supports;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Bot.Schema;
using Xunit;
using static Bot.Integration.Tests.Clockify.ClockifyConsts;

namespace Bot.Integration.Tests.Clockify
{
    [Collection(nameof(ClockifyCollection))]
    public class ClockifyWriteApiTest
    {
        private readonly ClockifyFixture _clockifyFixture;

        public ClockifyWriteApiTest(ClockifyFixture clockifyFixture)
        {
            _clockifyFixture = clockifyFixture;
        }

        [Fact]
        public async void AddTimeEntry_ApiKeyIsValidAndRequestBodyIsValid_ShouldAddTimeEntry()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());
            var clockifyClient = new RichClockifyClient(ClockifyApiKey);

            var now = DateTimeOffset.UtcNow;
            var newTimeEntry = new TimeEntryReq
            (
                _clockifyFixture.ProjectWithoutTasks().Id,
                now,
                end: now.AddHours(8)
            );

            var addedTimeEntry =
                await clockifyService.AddTimeEntryAsync(ClockifyApiKey, ClockifyWorkspaceId, newTimeEntry);

            addedTimeEntry.Should().NotBeNull();
            addedTimeEntry.TimeInterval.Start.Should().BeCloseTo(now, 5.Seconds());

            // cleanup
            var timeEntryDeleted =
                await clockifyClient.DeleteTimeEntryAsync(ClockifyWorkspaceId, addedTimeEntry.Id);
            timeEntryDeleted.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async void DeleteTimeEntry_ApiKeyIsValidAndTimeEntryExist_ShouldDeleteChosenTimeEntry()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());
            var clockifyClient = new RichClockifyClient(ClockifyApiKey);

            var now = DateTimeOffset.UtcNow;
            var newTimeEntry = new TimeEntryReq
            (
                _clockifyFixture.ProjectWithoutTasks().Id,
                now,
                end: now.AddHours(8)
            );

            var createdTimeEntry = await clockifyClient.CreateTimeEntryAsync(ClockifyWorkspaceId, 
                ClockifyModelFactory.ToTimeEntryRequest(newTimeEntry));
            createdTimeEntry.Data.Id.Should().NotBeNullOrWhiteSpace();

            string timeEntryToDelete = createdTimeEntry.Data.Id;

            Func<Task> action = () =>
                clockifyService.DeleteTimeEntry(ClockifyApiKey, ClockifyWorkspaceId, timeEntryToDelete);

            await action.Should().NotThrowAsync<ErrorResponseException>();
        }
    }
}