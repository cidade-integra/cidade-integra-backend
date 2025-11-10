using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Domain.Validation;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace CidadeIntegra.Domain.Test.Entities
{
    public class ReportTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var category = "Infrastructure";
            var title = "Buraco na rua";
            var description = "Existe um buraco grande na rua principal.";
            var isAnonymous = false;
            var status = "pending";

            // Act
            var report = new Report(userId, category, title, description, isAnonymous, status);

            // Assert
            report.Should().NotBeNull();
            report.UserId.Should().Be(userId);
            report.Category.Should().Be(category);
            report.Title.Should().Be(title);
            report.Description.Should().Be(description);
            report.IsAnonymous.Should().Be(isAnonymous);
            report.Status.Should().Be(status);
            report.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
            report.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData("", "Valid Title", "Valid Description", "pending")] // Empty category
        [InlineData("Category", "", "Valid Description", "pending")]      // Empty title
        [InlineData("Category", "Title", "", "pending")]                  // Empty description
        [InlineData("Category", "Title", "Description", "")]             // Empty status
        [InlineData("Category", "Title", "Description", "invalid")]      // Invalid status
        public void Constructor_WithInvalidParameters_ShouldThrowDomainException(
            string category, string title, string description, string status)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var isAnonymous = false;

            // Act
            Action act = () => new Report(userId, category, title, description, isAnonymous, status);

            // Assert
            act.Should().Throw<DomainExceptionValidation>();
        }

        [Fact]
        public void Constructor_WithEmptyUserId_ShouldThrowDomainException()
        {
            // Arrange
            var userId = Guid.Empty;
            var category = "Category";
            var title = "Title";
            var description = "Description";
            var isAnonymous = false;
            var status = "pending";

            // Act
            Action act = () => new Report(userId, category, title, description, isAnonymous, status);

            // Assert
            act.Should().Throw<DomainExceptionValidation>()
                .WithMessage("UserId must be provided.");
        }

        [Fact]
        public void ValidateResolvedAt_WithResolvedBeforeCreated_ShouldThrowValidationException()
        {
            // Arrange
            var report = new Report(Guid.NewGuid(), "Category", "Title", "Description", false);
            report.ResolvedAt = report.CreatedAt.AddMinutes(-10);

            // Act
            Action act = () => report.ValidateResolvedAt();

            // Assert
            act.Should().Throw<ValidationException>()
                .WithMessage("ResolvedAt cannot be before CreatedAt.");
        }

        [Fact]
        public void ValidateResolvedAt_WithResolvedAfterCreated_ShouldNotThrow()
        {
            // Arrange
            var report = new Report(Guid.NewGuid(), "Category", "Title", "Description", false);
            report.ResolvedAt = report.CreatedAt.AddMinutes(10);

            // Act
            Action act = () => report.ValidateResolvedAt();

            // Assert
            act.Should().NotThrow();
        }
    }
}
