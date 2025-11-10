using CidadeIntegra.Application.Interfaces.Repositories;
using CidadeIntegra.Application.Services;
using CidadeIntegra.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CidadeIntegra.Application.Test.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<UserService>>();
            _userService = new UserService(_userRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User(Guid.NewGuid(), "Test User", email, null, null, "user", "active", DateTimeOffset.UtcNow);
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetByEmailAsync(email);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
            _userRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "missing@example.com";
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetByEmailAsync(email);

            // Assert
            result.Should().BeNull();
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(email)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User(Guid.NewGuid(), "User1", "u1@example.com", null, null, "user", "active", DateTimeOffset.UtcNow),
                new User(Guid.NewGuid(), "User2", "u2@example.com", null, null, "user", "active", DateTimeOffset.UtcNow)
            };
            _userRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(users);

            // Act
            var result = await _userService.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallRepositoryAdd()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "Test User", "test@example.com", null, null, "user", "active", DateTimeOffset.UtcNow);

            // Act
            await _userService.CreateAsync(user);

            // Assert
            _userRepositoryMock.Verify(x => x.AddAsync(user), Times.Once);
            _userRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldThrow_WhenRepositoryThrows()
        {
            // Arrange
            var email = "error@example.com";
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ThrowsAsync(new Exception("DB error"));

            // Act
            Func<Task> act = async () => await _userService.GetByEmailAsync(email);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("DB error");
        }
    }
}