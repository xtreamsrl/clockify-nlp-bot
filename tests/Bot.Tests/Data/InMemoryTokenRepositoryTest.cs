using Bot.Data;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Bot.Tests.Data
{
    public class InMemoryTokenRepositoryTest
    {
        [Fact]
        public async void ReadAsync_GivenNullId_ShouldThrowArgumentNullException()
        {
            var inMemoryTokenRepository = new InMemoryTokenRepository();

            Func<Task> action = () =>
                inMemoryTokenRepository.ReadAsync(null);

            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async void ReadAsync_GivenTokenDoesNotExist_ShouldThrowTokenNotFoundException()
        {
            var inMemoryTokenRepository = new InMemoryTokenRepository();

            Func<Task> action = () =>
                inMemoryTokenRepository.ReadAsync("doesnotexist");

            await action.Should().ThrowAsync<TokenNotFoundException>();
        }

        [Fact]
        public async void ReadAsync_GivenTokenExist_ShouldReturnTokenData()
        {
            var expectedTokenData = new TokenData("UnitTestId", "UnitTestValue");
            var inMemoryTokenRepository = new InMemoryTokenRepository();
            await inMemoryTokenRepository.WriteAsync(expectedTokenData.Value, expectedTokenData.Id);

            var tokenData = await inMemoryTokenRepository.ReadAsync(expectedTokenData.Id);

            tokenData.Should().BeEquivalentTo(expectedTokenData);
        }

        [Fact]
        public async void WriteAsync_GivenNullValue_ShouldThrowArgumentException()
        {
            var inMemoryTokenRepository = new InMemoryTokenRepository();

            Func<Task> action = () =>
                inMemoryTokenRepository.WriteAsync(null);

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async void WriteAsync_GivenNullId_ShouldGenerateRandomId()
        {
            var expectedValue = "UnitTestValue";
            var inMemoryTokenRepository = new InMemoryTokenRepository();

            var tokenData = await inMemoryTokenRepository.WriteAsync(expectedValue);

            tokenData.Should().NotBeNull();
            tokenData.Id.Should().NotBeEmpty();
            tokenData.Value.Should().Be(expectedValue);
        }

        [Fact]
        public async void WriteAsync_GivenTokenDoesNotExist_ShouldAddAndReturnTokenData()
        {
            var expectedTokenData = new TokenData("UnitTestId", "UnitTestValue");
            var inMemoryTokenRepository = new InMemoryTokenRepository();

            var tokenData = await inMemoryTokenRepository.WriteAsync(expectedTokenData.Value, expectedTokenData.Id);

            tokenData.Should().BeEquivalentTo(expectedTokenData);

            var readTokenData = await inMemoryTokenRepository.ReadAsync(tokenData.Id);
            readTokenData.Should().BeEquivalentTo(tokenData);
        }


        [Fact]
        public async void WriteAsync_GivenTokenDoesExist_ShouldUpdateAndReturnTokenData()
        {
            var expectedTokenData = new TokenData("UnitTestId", "UnitTestValue");
            var inMemoryTokenRepository = new InMemoryTokenRepository();

            await inMemoryTokenRepository.WriteAsync(expectedTokenData.Value, expectedTokenData.Id);

            var expectedValue = "UpdatedUnitTestValue";

            var tokenData = await inMemoryTokenRepository.WriteAsync(expectedValue, expectedTokenData.Id);

            tokenData.Id.Should().Be(expectedTokenData.Id);
            tokenData.Value.Should().Be(expectedValue);
        }
    }
}
