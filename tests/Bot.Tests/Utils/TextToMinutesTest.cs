using Bot.Clockify.Fill;
using Bot.Utils;
using FluentAssertions;
using Xunit;

namespace Bot.Tests.Utils
{
    public class TextToMinutesTest
    {
        [Fact]
        public void TextToMinutes_10Hours()
        {
            TextToMinutes.ToMinutes("10 hours").Should().Be(10 * 60);
        }

        [Fact]
        public void TextToMinutes_2Days()
        {
            TextToMinutes.ToMinutes("2 days").Should().Be(2 * 24 * 60);
        }

        [Fact]
        public void TextToMinutes_1Year()
        {
            TextToMinutes.ToMinutes("1 year").Should().Be(365 * 24 * 60);
        }
    }
}