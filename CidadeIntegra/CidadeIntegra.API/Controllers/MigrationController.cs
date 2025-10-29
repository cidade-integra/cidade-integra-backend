using CidadeIntegra.Application.Interfaces.Services;
using CidadeIntegra.Domain.Entities;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CidadeIntegra.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MigrationController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IReportService _reportService;
        private readonly ILogger<MigrationController> _logger;
        private readonly FirestoreDb _firestore;

        public MigrationController(
            IUserService userService,
            IReportService reportService,
            ILogger<MigrationController> logger,
            IConfiguration configuration)
        {
            _userService = userService;
            _reportService = reportService;
            _logger = logger;

            var projectId = configuration["Firebase:ProjectId"];
            var credentialsPath = configuration["Firebase:ServiceAccountPath"];

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
            _firestore = FirestoreDb.Create(projectId);
        }

        [HttpPost("run")]
        public async Task<IActionResult> RunMigration()
        {
            try
            {
                _logger.LogInformation("Iniciando processo de migração...");

                await MigrateUsersAsync();
                await MigrateReportsAsync();

                _logger.LogInformation("Migração concluída com sucesso.");
                return Ok(new { message = "Migração concluída com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o processo de migração.");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task MigrateUsersAsync()
        {
            _logger.LogInformation("Migrando coleção: users...");

            var usersCollection = _firestore.Collection("users");
            var snapshot = await usersCollection.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    DisplayName = data.GetValueOrDefault("displayName", string.Empty)?.ToString() ?? "",
                    Email = data.GetValueOrDefault("email", string.Empty)?.ToString() ?? "",
                    PhotoUrl = data.GetValueOrDefault("photoURL", string.Empty)?.ToString() ?? "",
                    Region = data.GetValueOrDefault("region", string.Empty)?.ToString() ?? "",
                    Role = data.GetValueOrDefault("role", "user")?.ToString() ?? "user",
                    Status = data.GetValueOrDefault("status", "active")?.ToString() ?? "active",
                    Score = Convert.ToInt32(data.GetValueOrDefault("score", 0)),
                    ReportCount = Convert.ToInt32(data.GetValueOrDefault("reportCount", 0)),
                    Verified = Convert.ToBoolean(data.GetValueOrDefault("verified", false)),
                    CreatedAt = ParseDate(data.GetValueOrDefault("createdAt")),
                    LastLoginAt = ParseDate(data.GetValueOrDefault("lastLoginAt"))
                };

                // Evita duplicação por e-mail
                var existing = await _userService.GetByEmailAsync(user.Email);
                if (existing == null)
                {
                    await _userService.CreateAsync(user);
                    _logger.LogInformation($"Usuário migrado: {user.DisplayName}");
                }
            }

            _logger.LogInformation("Migração de usuários concluída.");
        }

        private async Task MigrateReportsAsync()
        {
            _logger.LogInformation("Migrando coleção: reports...");

            var reportsCollection = _firestore.Collection("reports");
            var snapshot = await reportsCollection.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();

                var userIdFirestore = data.GetValueOrDefault("userId", null)?.ToString();

                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    Category = data.GetValueOrDefault("category", "outros")?.ToString() ?? "outros",
                    Title = data.GetValueOrDefault("title", string.Empty)?.ToString() ?? "",
                    Description = data.GetValueOrDefault("description", string.Empty)?.ToString() ?? "",
                    Status = data.GetValueOrDefault("status", "pending")?.ToString() ?? "pending",
                    IsAnonymous = Convert.ToBoolean(data.GetValueOrDefault("isAnonymous", false)),
                    ImageUrl1 = ExtractFirstImageUrl(data),
                    CreatedAt = ParseDate(data.GetValueOrDefault("createdAt")),
                    UpdatedAt = ParseDate(data.GetValueOrDefault("updatedAt")),
                    ResolvedAt = ParseDate(data.GetValueOrDefault("resolvedAt"))
                };

                // localização
                if (data.TryGetValue("location", out var locationObj) && locationObj is Dictionary<string, object> loc)
                {
                    report.Location = new ReportLocation
                    {
                        Id = Guid.NewGuid(),
                        Address = loc.GetValueOrDefault("address", "")?.ToString() ?? "",
                        Latitude = Convert.ToDecimal(loc.GetValueOrDefault("latitude", 0)),
                        Longitude = Convert.ToDecimal(loc.GetValueOrDefault("longitude", 0)),
                        PostalCode = loc.GetValueOrDefault("postalCode", "")?.ToString() ?? ""
                    };
                }

                await _reportService.CreateAsync(report);
                _logger.LogInformation($"Report migrado: {report.Title}");
            }

            _logger.LogInformation("Migração de reports concluída.");
        }

        private static DateTimeOffset ParseDate(object? dateObj)
        {
            if (dateObj is Timestamp ts)
                return ts.ToDateTimeOffset();

            if (DateTimeOffset.TryParse(dateObj?.ToString(), out var parsed))
                return parsed;

            return DateTimeOffset.UtcNow;
        }

        private static string ExtractFirstImageUrl(Dictionary<string, object> data)
        {
            if (data.TryGetValue("imagemUrls", out var urlsObj) && urlsObj is IEnumerable<object> urls)
                return urls.FirstOrDefault()?.ToString() ?? "";

            return "";
        }
    }
}