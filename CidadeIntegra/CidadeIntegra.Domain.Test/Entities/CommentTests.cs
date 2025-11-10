using System;
using System.ComponentModel.DataAnnotations;
using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Domain.Validation;
using Xunit;

namespace CidadeIntegra.Domain.Tests.Entities
{
    public class CommentTests
    {
        [Fact]
        public void Should_Create_Comment_When_Valid_Data()
        {
            var comment = new Comment(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "#FF5733",
                "Mensagem válida",
                "Administrador"
            );

            Assert.NotEqual(Guid.Empty, comment.Id);
            Assert.Equal("Mensagem válida", comment.Message);
            Assert.Equal("Administrador", comment.Role);
            Assert.Equal("#FF5733", comment.AvatarColor);
            Assert.True(comment.CreatedAt <= DateTimeOffset.UtcNow);
        }

        [Theory]
        [InlineData("", "Mensagem", "Role")]
        [InlineData(" ", "Mensagem", "Role")]
        [InlineData(null, "Mensagem", "Role")]
        public void Should_Throw_When_AvatarColor_Invalid(string avatarColor, string message, string role)
        {
            Assert.Throws<DomainExceptionValidation>(() =>
                new Comment(Guid.NewGuid(), Guid.NewGuid(), avatarColor, message, role));
        }

        [Theory]
        [InlineData("", "Avatar", "Role")]
        [InlineData(" ", "Avatar", "Role")]
        [InlineData(null, "Avatar", "Role")]
        public void Should_Throw_When_Message_Invalid(string message, string avatar, string role)
        {
            Assert.Throws<DomainExceptionValidation>(() =>
                new Comment(Guid.NewGuid(), Guid.NewGuid(), avatar, message, role));
        }

        [Fact]
        public void Should_Throw_When_Role_Empty()
        {
            Assert.Throws<DomainExceptionValidation>(() =>
                new Comment(Guid.NewGuid(), Guid.NewGuid(), "#000000", "Mensagem", ""));
        }

        [Fact]
        public void Should_Throw_When_ReportId_Empty()
        {
            Assert.Throws<DomainExceptionValidation>(() =>
                new Comment(Guid.Empty, Guid.NewGuid(), "#000000", "Mensagem", "User"));
        }

        [Fact]
        public void Should_Throw_When_AuthorId_Empty()
        {
            Assert.Throws<DomainExceptionValidation>(() =>
                new Comment(Guid.NewGuid(), Guid.Empty, "#000000", "Mensagem", "User"));
        }

        [Fact]
        public void Should_Throw_When_Message_Exceeds_Max_Length()
        {
            var message = new string('a', 501);
            Assert.Throws<DomainExceptionValidation>(() =>
                new Comment(Guid.NewGuid(), Guid.NewGuid(), "#000000", message, "User"));
        }

        [Fact]
        public void Should_Throw_When_CreatedAt_In_Future()
        {
            var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), "#111111", "Mensagem válida", "User");

            var prop = typeof(Comment).GetProperty("CreatedAt");
            prop!.SetValue(comment, DateTimeOffset.UtcNow.AddDays(1));

            Assert.Throws<ValidationException>(() => comment.Validate());
        }
    }
}
