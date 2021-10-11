using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;
using Bot.Data;
using Moq;
using Azure.Security.KeyVault.Secrets;
using System.Threading;
using Azure;

namespace Bot.Tests.Data
{
    public class TokenRepositoryTest
    {
        private const int _cacheSeconds = 10;

        private readonly Mock<SecretClient> _mockSecretClient;
        private readonly Mock<Response> _mockResponse;
        private readonly MemoryCacheStub _memoryCacheStub;

        private readonly TokenRepository _sut;

        public TokenRepositoryTest()
        {
            _mockSecretClient = new Mock<SecretClient>();
            _mockResponse = new Mock<Response>();
            _memoryCacheStub = new MemoryCacheStub();
            _sut = new TokenRepository(_mockSecretClient.Object, _memoryCacheStub, _cacheSeconds);
        }

        [Fact]
        public async void ReadAsync_GivenNullId_ShouldThrowArgumentNullException()
        {
            Func<Task> action = () =>
                _sut.ReadAsync(null);

            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async void ReadAsync_GivenTokenDataInCache_ShouldReturnTokenDataFromCache()
        {
            var expectedTokenData = new TokenData("UnitTestId", "UnitTestValue");
            _memoryCacheStub.Set(expectedTokenData.Id, expectedTokenData);

            var tokenData = await _sut.ReadAsync(expectedTokenData.Id);

            tokenData.Should().BeEquivalentTo(expectedTokenData);

            _mockSecretClient.Verify(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async void ReadAsync_GivenTokenDoesNotExist_ShouldThrowTokenNotFoundException()
        {
            _mockSecretClient
                .Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(404, ""));

            Func<Task> action = () =>
                _sut.ReadAsync("doesnotexist");

            await action.Should().ThrowAsync<TokenNotFoundException>();
        }

        [Fact]
        public async void ReadAsync_GivenSecretFound_ShouldCacheAndReturnTokenData()
        {
            var expectedTokenData = new TokenData("UnitTestId", "UnitTestValue");
            _mockSecretClient
                .Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(new KeyVaultSecret(expectedTokenData.Id, expectedTokenData.Value), _mockResponse.Object));

            var tokenData = await _sut.ReadAsync(expectedTokenData.Id);

            tokenData.Should().BeEquivalentTo(expectedTokenData);
            _memoryCacheStub.TryGetValue(expectedTokenData.Id, out var value).Should().BeTrue();
        }

        [Fact]
        public async void WriteAsync_GivenNullValue_ShouldThrowArgumentException()
        {
            Func<Task> action = () =>
                _sut.WriteAsync(null);

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async void WriteAsync_GivenTokenDoesNotExist_ShouldAddAndCacheTokenData()
        {
            var expectedTokenData = new TokenData("UnitTestId", "UnitTestValue");
            _mockSecretClient
                .Setup(x => x.SetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(new KeyVaultSecret(expectedTokenData.Id, expectedTokenData.Value), _mockResponse.Object));

            var tokenData = await _sut.WriteAsync(expectedTokenData.Value, expectedTokenData.Id);

            tokenData.Should().BeEquivalentTo(expectedTokenData);
            _memoryCacheStub.TryGetValue(expectedTokenData.Id, out var value).Should().BeTrue();
        }

        [Fact]
        public async void WriteAsync_GivenNullId_ShouldGenerateRandomId()
        {
            var expectedTokenData = new TokenData("UnitTestId", "UnitTestValue");
            _mockSecretClient
                .Setup(x => x.SetSecretAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(new KeyVaultSecret(expectedTokenData.Id, expectedTokenData.Value), _mockResponse.Object));

            var tokenData = await _sut.WriteAsync(expectedTokenData.Value);

            tokenData.Should().BeEquivalentTo(expectedTokenData);
            _memoryCacheStub.TryGetValue(expectedTokenData.Id, out var value).Should().BeTrue();
        }
    }
}
