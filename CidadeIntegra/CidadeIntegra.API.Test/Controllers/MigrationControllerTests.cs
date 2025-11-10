using CidadeIntegra.Application.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using Xunit;

namespace CidadeIntegra.API.Test.Controllers
{
    public class MigrationControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly Mock<IMigrationService> _migrationServiceMock;

        public MigrationControllerTests(WebApplicationFactory<Program> factory)
        {
            // Criamos o mock do MigrationService
            _migrationServiceMock = new Mock<IMigrationService>();
            _migrationServiceMock.Setup(m => m.RunMigrationAsync()).Returns(Task.CompletedTask);

            // Configuramos a WebApplicationFactory para usar o mock
            var webFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove a implementação real do MigrationService
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IMigrationService));
                    if (descriptor != null) services.Remove(descriptor);

                    // Adiciona o mock
                    services.AddSingleton(_migrationServiceMock.Object);
                });
            });

            // Criamos o HttpClient que chamará o controller
            _client = webFactory.CreateClient();
        }

        [Fact]
        public async Task RunMigration_WhenServiceThrowsException_Returns500()
        {
            // Arrange
            Environment.SetEnvironmentVariable("MIGRATION_API_KEY", "valid-key");
            _client.DefaultRequestHeaders.Add("x-api-key", "valid-key");

            _migrationServiceMock.Setup(m => m.RunMigrationAsync())
                                 .ThrowsAsync(new InvalidOperationException("Falha na migração"));

            // Act
            var response = await _client.PostAsync("/api/migration/run", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            _migrationServiceMock.Verify(m => m.RunMigrationAsync(), Times.Once);
        }
    }
}