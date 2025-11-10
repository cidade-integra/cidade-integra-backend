using System;
using System.ComponentModel.DataAnnotations;
using CidadeIntegra.Domain.Entities;
using Xunit;

namespace CidadeIntegra.Domain.Tests.Entities
{
    public class LogTests
    {
        [Fact]
        public void Create_ValidData_ShouldCreateLogSuccessfully()
        {
            // Arrange
            var log = Log.Create("Info", "Operation completed successfully");

            // Assert
            Assert.NotEqual(Guid.Empty, log.Id);
            Assert.Equal("Info", log.Level);
            Assert.Equal("Operation completed successfully", log.Message);
            Assert.NotEqual(default, log.TimeStamp);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validate_InvalidLevel_ShouldThrowValidationException(string invalidLevel)
        {
            // Arrange
            var log = new Log
            {
                Id = Guid.NewGuid(),
                Level = invalidLevel ?? string.Empty,
                Message = "Some message",
                TimeStamp = DateTimeOffset.UtcNow
            };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => log.Validate());
            Assert.Equal("Level is required.", ex.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validate_InvalidMessage_ShouldThrowValidationException(string invalidMessage)
        {
            // Arrange
            var log = new Log
            {
                Id = Guid.NewGuid(),
                Level = "Error",
                Message = invalidMessage ?? string.Empty,
                TimeStamp = DateTimeOffset.UtcNow
            };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => log.Validate());
            Assert.Equal("Message is required.", ex.Message);
        }

        [Fact]
        public void Validate_EmptyId_ShouldThrowValidationException()
        {
            // Arrange
            var log = new Log
            {
                Id = Guid.Empty,
                Level = "Warning",
                Message = "Id missing",
                TimeStamp = DateTimeOffset.UtcNow
            };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => log.Validate());
            Assert.Equal("Id cannot be empty.", ex.Message);
        }

        [Fact]
        public void Validate_DefaultTimeStamp_ShouldThrowValidationException()
        {
            // Arrange
            var log = new Log
            {
                Id = Guid.NewGuid(),
                Level = "Info",
                Message = "Missing timestamp",
                TimeStamp = default
            };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => log.Validate());
            Assert.Equal("Timestamp must be set.", ex.Message);
        }

        [Fact]
        public void Validate_LevelExceedsMaxLength_ShouldThrowValidationException()
        {
            // Arrange
            var longLevel = new string('A', 51);
            var log = new Log
            {
                Id = Guid.NewGuid(),
                Level = longLevel,
                Message = "Too long level",
                TimeStamp = DateTimeOffset.UtcNow
            };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => log.Validate());
            Assert.Equal("Level exceeds maximum length of 50 characters.", ex.Message);
        }

        [Fact]
        public void Validate_MessageExceedsMaxLength_ShouldThrowValidationException()
        {
            // Arrange
            var longMessage = new string('B', 2001);
            var log = new Log
            {
                Id = Guid.NewGuid(),
                Level = "Error",
                Message = longMessage,
                TimeStamp = DateTimeOffset.UtcNow
            };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => log.Validate());
            Assert.Equal("Message exceeds maximum length of 2000 characters.", ex.Message);
        }
    }
}
