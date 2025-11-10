using CidadeIntegra.Application.Services;
using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Infra.Data.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CidadeIntegra.Application.Test.Services
{
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _commentRepositoryMock;
        private readonly Mock<ILogger<CommentService>> _loggerMock;
        private readonly CommentService _commentService;

        public CommentServiceTests()
        {
            _commentRepositoryMock = new Mock<ICommentRepository>();
            _loggerMock = new Mock<ILogger<CommentService>>();
            _commentService = new CommentService(_commentRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnComment_WhenCommentExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var comment = new Comment(id, Guid.NewGuid(), Guid.NewGuid(), "blue", "Message", "user", DateTimeOffset.UtcNow);
            _commentRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(comment);

            // Act
            var result = await _commentService.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            _commentRepositoryMock.Verify(x => x.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenCommentDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            _commentRepositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Comment?)null);

            // Act
            var result = await _commentService.GetByIdAsync(id);

            // Assert
            result.Should().BeNull();
            _commentRepositoryMock.Verify(x => x.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetByReportIdAsync_ShouldReturnComments_WhenCommentsExist()
        {
            // Arrange
            var reportId = Guid.NewGuid();
            var comments = new List<Comment>
            {
                new Comment(Guid.NewGuid(), reportId, Guid.NewGuid(), "red", "Message1", "user", DateTimeOffset.UtcNow),
                new Comment(Guid.NewGuid(), reportId, Guid.NewGuid(), "blue", "Message2", "admin", DateTimeOffset.UtcNow)
            };
            _commentRepositoryMock.Setup(x => x.GetByReportIdAsync(reportId)).ReturnsAsync(comments);

            // Act
            var result = await _commentService.GetByReportIdAsync(reportId);

            // Assert
            result.Should().HaveCount(2);
            _commentRepositoryMock.Verify(x => x.GetByReportIdAsync(reportId), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallAddAndSaveChanges()
        {
            // Arrange
            var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "green", "New comment", "user", DateTimeOffset.UtcNow);

            // Act
            await _commentService.CreateAsync(comment);

            // Assert
            _commentRepositoryMock.Verify(x => x.AddAsync(comment), Times.Once);
            _commentRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallUpdateAndSaveChanges()
        {
            // Arrange
            var comment = new Comment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "yellow", "Update comment", "user", DateTimeOffset.UtcNow);

            // Act
            await _commentService.UpdateAsync(comment);

            // Assert
            _commentRepositoryMock.Verify(x => x.UpdateAsync(comment), Times.Once);
            _commentRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteAndSaveChanges()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            await _commentService.DeleteAsync(id);

            // Assert
            _commentRepositoryMock.Verify(x => x.DeleteAsync(id), Times.Once);
            _commentRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}