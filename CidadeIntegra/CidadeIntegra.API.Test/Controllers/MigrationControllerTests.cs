using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using Xunit;

namespace CidadeIntegra.API.Test.Controllers
{
    public class MigrationControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public MigrationControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RunMigration_WithoutApiKey_Returns403()
        {
            var response = await _client.PostAsync("/api/migration/run", null);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task RunMigration_WithInvalidApiKey_Returns403()
        {
            _client.DefaultRequestHeaders.Add("x-api-key", "wrong-key");
            var response = await _client.PostAsync("/api/migration/run", null);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task RunMigration_WithValidApiKey_Returns200()
        {
            Environment.SetEnvironmentVariable("MIGRATION_API_KEY", "valid-key");
            _client.DefaultRequestHeaders.Add("x-api-key", "valid-key");

            var response = await _client.PostAsync("/api/migration/run", null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

}