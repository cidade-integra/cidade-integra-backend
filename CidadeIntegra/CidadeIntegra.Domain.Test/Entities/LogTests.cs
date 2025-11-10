using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Domain.Validation;
using FluentAssertions;
using Xunit;

namespace CidadeIntegra.Domain.Test.Entities
{
    public class LogTests
    {
        [Fact]
        public void Create_ValidParameters_ShouldReturnLog()
        {
            // Arrange
            var level = "Information";
            var message = "This is a test log.";
            var exception = "Exception details";

            // Act
            var log = Log.Create(level, message, exception);

            // Assert
            log.Should().NotBeNull();
            log.Id.Should().NotBeEmpty();
            log.Level.Should().Be(level);
            log.Message.Should().Be(message);
            log.Exception.Should().Be(exception);
            log.TimeStamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData("", "Valid message")]
        [InlineData(" ", "Valid message")]
        [InlineData("Valid level", "")]
        [InlineData("Valid level", " ")]
        public void Create_InvalidParameters_ShouldThrowDomainException(string level, string message)
        {
            // Act
            Action act = () => Log.Create(level, message);

            // Assert
            act.Should().Throw<DomainExceptionValidation>();
        }

        [Fact]
        public void Create_LevelTooLong_ShouldThrowDomainException()
        {
            // Arrange
            var level = new string('A', 51);
            var message = "Valid message";

            // Act
            Action act = () => Log.Create(level, message);

            // Assert
            act.Should().Throw<DomainExceptionValidation>()
                .WithMessage("Level exceeds maximum length of 50 characters.");
        }

        [Fact]
        public void Create_MessageTooLong_ShouldThrowDomainException()
        {
            // Arrange
            var level = "Info";
            var message = new string('B', 2001);

            // Act
            Action act = () => Log.Create(level, message);

            // Assert
            act.Should().Throw<DomainExceptionValidation>()
                .WithMessage("Message exceeds maximum length of 2000 characters.");
        }
    }
}