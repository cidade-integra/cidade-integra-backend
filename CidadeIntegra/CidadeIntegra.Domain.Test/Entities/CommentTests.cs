using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Domain.Validation;
using FluentAssertions;
using Xunit;

namespace CidadeIntegra.Domain.Test.Entities
{
    public class CommentTests
    {
        [Fact]
        public void Create_ValidParameters_ShouldReturnComment()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var avatarColor = "red";
            var message = "This is a comment";
            var role = "user";

            // Act
            var comment = Comment.Create(reportId, authorId, avatarColor, message, role);

            // Assert
            comment.Should().NotBeNull();
            comment.Id.Should().NotBeEmpty();
            comment.ReportId.Should().Be(reportId);
            comment.AuthorId.Should().Be(authorId);
            comment.AvatarColor.Should().Be(avatarColor);
            comment.Message.Should().Be(message);
            comment.Role.Should().Be(role);
            comment.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData("", "Message", "user")]
        [InlineData("Avatar", "", "user")]
        [InlineData("Avatar", "Message", "")]
        public void Create_InvalidParameters_ShouldThrowDomainException(string avatar, string message, string role)
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var authorId = Guid.NewGuid();

            // Act
            Action act = () => Comment.Create(reportId, authorId, avatar, message, role);

            // Assert
            act.Should().Throw<DomainExceptionValidation>();
        }

        [Fact]
        public void Create_MessageTooLong_ShouldThrowDomainException()
        {
            var reportId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var avatarColor = "red";
            var message = new string('A', 501);
            var role = "user";

            Action act = () => Comment.Create(reportId, authorId, avatarColor, message, role);

            act.Should().Throw<DomainExceptionValidation>()
                .WithMessage("Message length cannot exceed 500 characters.");
        }
    }
}