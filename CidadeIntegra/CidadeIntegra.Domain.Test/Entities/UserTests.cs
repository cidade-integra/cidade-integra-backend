using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Domain.Validation;
using FluentAssertions;
using Xunit;

namespace CidadeIntegra.Domain.Test.Entities
{
    public class UserTests
    {
        #region Criação de usuário

        [Fact]
        public void Constructor_Should_CreateUser_When_ValidData()
        {
            // Arrange
            var id = Guid.NewGuid();
            var displayName = "John Doe";
            var email = "john@example.com";
            var photoUrl = "http://photo.com/john.png";
            var region = "SP";
            var role = "user";
            var status = "active";
            var createdAt = DateTimeOffset.UtcNow;

            // Act
            var user = new User(id, displayName, email, photoUrl, region, role, status, createdAt);

            // Assert
            user.Id.Should().Be(id);
            user.DisplayName.Should().Be(displayName);
            user.Email.Should().Be(email.ToLowerInvariant());
            user.PhotoUrl.Should().Be(photoUrl);
            user.Region.Should().Be(region);
            user.Role.Should().Be(role);
            user.Status.Should().Be(status);
            user.CreatedAt.Should().Be(createdAt);
            user.LastLoginAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
            user.ReportCount.Should().Be(0);
            user.Score.Should().Be(0);
            user.Verified.Should().BeFalse();
        }

        [Theory]
        [InlineData("", "john@example.com", "DisplayName is required.")]
        [InlineData("John", "", "Email is required.")]
        [InlineData("John", "invalid-email", "Email must be valid.")]
        public void Constructor_Should_ThrowDomainException_When_InvalidData(string displayName, string email, string expectedMessage)
        {
            // Arrange
            var id = Guid.NewGuid();
            var createdAt = DateTimeOffset.UtcNow;

            // Act
            Action act = () => new User(id, displayName, email, null, null, "user", "active", createdAt);

            // Assert
            act.Should().Throw<DomainExceptionValidation>()
               .WithMessage(expectedMessage);
        }

        #endregion

        #region Atualização de perfil

        [Fact]
        public void UpdateProfile_Should_UpdateFields_When_ValidData()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "John", "john@example.com", null, null, "user", "active", DateTimeOffset.UtcNow);
            var newName = "Jane Doe";
            var newEmail = "jane@example.com";
            var newPhoto = "http://photo.com/jane.png";
            var newRegion = "RJ";

            // Act
            user.UpdateProfile(newName, newEmail, newPhoto, newRegion);

            // Assert
            user.DisplayName.Should().Be(newName);
            user.Email.Should().Be(newEmail.ToLowerInvariant());
            user.PhotoUrl.Should().Be(newPhoto);
            user.Region.Should().Be(newRegion);
        }

        [Fact]
        public void UpdateProfile_Should_Throw_When_InvalidEmail()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "John", "john@example.com", null, null, "user", "active", DateTimeOffset.UtcNow);

            // Act
            Action act = () => user.UpdateProfile("John", "invalid-email", null, null);

            // Assert
            act.Should().Throw<DomainExceptionValidation>()
               .WithMessage("Email must be valid.");
        }

        #endregion

        #region Alteração de role/status

        [Fact]
        public void ChangeRole_Should_UpdateRole_When_Valid()
        {
            var user = new User(Guid.NewGuid(), "John", "john@example.com", null, null, "user", "active", DateTimeOffset.UtcNow);
            user.ChangeRole("admin");
            user.Role.Should().Be("admin");
        }

        [Fact]
        public void ChangeStatus_Should_UpdateStatus_When_Valid()
        {
            var user = new User(Guid.NewGuid(), "John", "john@example.com", null, null, "user", "active", DateTimeOffset.UtcNow);
            user.ChangeStatus("inactive");
            user.Status.Should().Be("inactive");
        }

        #endregion

        #region Estatísticas

        [Fact]
        public void SetStats_Should_UpdateStats_When_Valid()
        {
            var user = new User(Guid.NewGuid(), "John", "john@example.com", null, null, "user", "active", DateTimeOffset.UtcNow);
            user.SetStats(10, 5, true);

            user.Score.Should().Be(10);
            user.ReportCount.Should().Be(5);
            user.Verified.Should().BeTrue();
        }

        [Fact]
        public void SetStats_Should_Throw_When_NegativeValues()
        {
            var user = new User(Guid.NewGuid(), "John", "john@example.com", null, null, "user", "active", DateTimeOffset.UtcNow);

            Action act = () => user.SetStats(-1, 0, true);
            act.Should().Throw<DomainExceptionValidation>()
                .WithMessage("Score cannot be negative.");

            act = () => user.SetStats(0, -5, true);
            act.Should().Throw<DomainExceptionValidation>()
                .WithMessage("ReportCount cannot be negative.");
        }

        #endregion
    }
}