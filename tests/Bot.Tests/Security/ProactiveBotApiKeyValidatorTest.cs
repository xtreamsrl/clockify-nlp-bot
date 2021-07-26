using System;
using Bot.Security;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bot.Tests.Security
{
    public class ProactiveBotApiKeyValidatorTest
    {
        
        [Fact]
        public void Validate_ClientApiKeyDoNotMathWithServeApiKey_ThrowInvalidApiKeyException()
        {
            // Stub
            const string serverApiKey = "asjdhajshdkhasd";
            var providerMock = new Mock<IProactiveApiKeyProvider>();
            providerMock.Setup(p => p.GetApiKey())
                .Returns(serverApiKey);

            const string invalidClientApiKey = "invalid";
            
            var validator = new ProactiveBotApiKeyValidator(providerMock.Object);
            Action act = () => validator.Validate(invalidClientApiKey);

            act.Should().ThrowExactly<InvalidApiKeyException>();
        }
        
        [Fact]
        public void  Validate_ClientApiKeyIsEmptyWhileServerApiKeyIsSet_ThrowMissingApiKeyException()
        {
            // Stub
            const string serverApiKey = "asjdhajshdkhasd";
            var providerMock = new Mock<IProactiveApiKeyProvider>();
            providerMock.Setup(p => p.GetApiKey())
                .Returns(serverApiKey);

            const string emptyClientApiKey = "";
            
            var validator = new ProactiveBotApiKeyValidator(providerMock.Object);
            Action act = () => validator.Validate(emptyClientApiKey);

            act.Should().ThrowExactly<MissingApiKeyException>();
        }
        
        [Fact]
        public void  Validate_ClientApiKeyIsValid_DoNotThrowException()
        {
            // Stub
            const string serverApiKey = "asjdhajshdkhasd";
            var providerMock = new Mock<IProactiveApiKeyProvider>();
            providerMock.Setup(p => p.GetApiKey())
                .Returns(serverApiKey);

            const string emptyClientApiKey = serverApiKey;
            
            var validator = new ProactiveBotApiKeyValidator(providerMock.Object);
            Action act = () => validator.Validate(emptyClientApiKey);

            act.Should().NotThrow();
        }
        
        [Fact]
        public void  Validate_ProactiveApiKeySecurityDisabled_DoNotThrowException()
        {
            // Stub
            const string emptyServerApiKey = "";
            var providerMock = new Mock<IProactiveApiKeyProvider>();
            providerMock.Setup(p => p.GetApiKey())
                .Returns(emptyServerApiKey);

            const string randomClientApiKey = "lsldklasdlkadslk";
            const string emptyClientApiKey = "";
            
            var validator = new ProactiveBotApiKeyValidator(providerMock.Object);
            Action act1 = () => validator.Validate(emptyClientApiKey);
            Action act2 = () => validator.Validate(randomClientApiKey);

            act1.Should().NotThrow();
            act2.Should().NotThrow();
        }
    }
}