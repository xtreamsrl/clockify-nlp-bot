using Xunit;

namespace Bot.Integration.Tests.Clockify.Supports
{
    [CollectionDefinition(nameof(ClockifyCollection))]
    public class ClockifyCollection : ICollectionFixture<ClockifyFixture> {}
}