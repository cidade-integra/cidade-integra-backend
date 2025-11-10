using CidadeIntegra.Application.Services;
using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Infra.Data.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CidadeIntegra.Application.Test.Services
{
    public class LogServiceTests
    {
        private readonly Mock<ILogRepository> _logRepositoryMock;
        private readonly Mock<ILogger<LogService>> _loggerMock;
        private readonly LogService _logService;

        public LogServiceTests()
        {
            _logRepositoryMock = new Mock<ILogRepository>();
            _loggerMock = new Mock<ILogger<LogService>>();
            _logService = new LogService(_logRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateLogAsync_ShouldSaveLog_WhenValid()
        {
            // Arrange
            var log = Log.Create("Information", "Teste de log válido");

            // Act
            await _logService.CreateLogAsync(log);

            // Assert
            _logRepositoryMock.Verify(x => x.AddAsync(log), Times.Once);
            _logRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);

            // Verifica log de informação via método base Log()
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Log gravado com sucesso")),
                    null,
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }


        [Fact]
        public async Task CreateLogAsync_ShouldThrowArgumentNullException_WhenLogIsNull()
        {
            // Act
            Func<Task> act = async () => await _logService.CreateLogAsync(null!);

            // Assert
            await act.Should()
                .ThrowAsync<ArgumentNullException>()
                .WithParameterName("log");

            _logRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Log>()), Times.Never);
        }

        [Fact]
        public async Task CreateLogAsync_ShouldHandleRepositoryException_AndLogError()
        {
            // Arrange
            var log = Log.Create("Error", "Erro esperado de teste");
            var exception = new InvalidOperationException("Erro simulado");

            _logRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Log>()))
                .ThrowsAsync(exception);

            // Act
            Func<Task> act = async () => await _logService.CreateLogAsync(log);

            // Assert — o serviço não lança exceção
            await act.Should().NotThrowAsync();

            // verifica se o logger registrou um LogError (via método base Log)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Erro ao gravar log no banco")),
                    exception,
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }
    }
}