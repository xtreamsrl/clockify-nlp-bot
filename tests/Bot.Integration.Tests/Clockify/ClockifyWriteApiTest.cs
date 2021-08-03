using System;
using System.Threading.Tasks;
using Bot.Clockify.Client;
using Clockify.Net.Models.TimeEntries;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Bot.Schema;
using Xunit;
using static Bot.Integration.Tests.Clockify.ClockifyConsts;

namespace Bot.Integration.Tests.Clockify
{
    public class ClockifyWriteApiTest
    {
        [Fact]
        public async void AddTimeEntry_ApiKeyIsValidAndRequestBodyIsValid_ShouldAddTimeEntry()
        {
            var clockifyService = new ClockifyService(new ClockifyClientFactory());
            var clockifyClient = new RichClockifyClient(ClockifyApiKey);

            var now = DateTimeOffset.UtcNow;
            var newTimeEntry = new TimeEntryRequest
            {
                Start = now,
                End = now.AddHours(8),
                ProjectId = "5efc6f19f833d7257bfa3c54"
            };

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
            var newTimeEntry = new TimeEntryRequest
            {
                Start = now,
                End = now.AddHours(8),
                ProjectId = "5efc6f19f833d7257bfa3c54"
            };

            var createdTimeEntry = await clockifyClient.CreateTimeEntryAsync(ClockifyWorkspaceId, newTimeEntry);
            createdTimeEntry.Data.Id.Should().NotBeNullOrWhiteSpace();

            var timeEntryToDelete = createdTimeEntry.Data.Id;

            Func<Task> action = () =>
                clockifyService.DeleteTimeEntry(ClockifyApiKey, ClockifyWorkspaceId, timeEntryToDelete);

            await action.Should().NotThrowAsync<ErrorResponseException>();
        }
    }
}