using CidadeIntegra.Application.Interfaces.Services;
using CidadeIntegra.Domain.Entities;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CidadeIntegra.Application.Services
{
    public class MigrationService : IMigrationService
    {
        private readonly IUserService _userService;
        private readonly IReportService _reportService;
        private readonly ILogger<MigrationService> _logger;
        private readonly FirestoreDb _firestore;

        public MigrationService(
            IUserService userService,
            IReportService reportService,
            ILogger<MigrationService> logger,
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

        public async Task RunMigrationAsync()
        {
            _logger.LogInformation("Iniciando processo de migração...");

            await MigrateUsersAsync();
            await MigrateReportsAsync();

            _logger.LogInformation("Migração concluída com sucesso!");
        }

        #region USERS
        private async Task MigrateUsersAsync()
        {
            _logger.LogInformation("Migrando coleção: users...");

            var usersCollection = _firestore.Collection("users");
            var snapshot = await usersCollection.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();
                var firebaseId = doc.Id;

                var existingUser = await _userService.GetByFirebaseIdAsync(firebaseId);

                var user = existingUser ?? new User { Id = existingUser?.Id ?? Guid.NewGuid() };

                // Atualiza ou preenche propriedades
                user.FirebaseId = firebaseId;
                user.DisplayName = data.GetValueOrDefault("displayName", string.Empty)?.ToString() ?? "";
                user.Email = data.GetValueOrDefault("email", string.Empty)?.ToString() ?? "";
                user.PhotoUrl = data.GetValueOrDefault("photoURL", string.Empty)?.ToString() ?? "";
                user.Region = data.GetValueOrDefault("region", string.Empty)?.ToString() ?? "";
                user.Role = data.GetValueOrDefault("role", "user")?.ToString() ?? "user";
                user.Status = data.GetValueOrDefault("status", "active")?.ToString() ?? "active";
                user.Score = Convert.ToInt32(data.GetValueOrDefault("score", 0));
                user.ReportCount = Convert.ToInt32(data.GetValueOrDefault("reportCount", 0));
                user.Verified = Convert.ToBoolean(data.GetValueOrDefault("verified", false));
                user.CreatedAt = ParseDate(data.GetValueOrDefault("createdAt"));
                user.LastLoginAt = ParseDate(data.GetValueOrDefault("lastLoginAt"));

                if (existingUser == null)
                {
                    await _userService.CreateAsync(user);
                    _logger.LogInformation($"Usuário criado: {user.DisplayName}");
                }
                else
                {
                    await _userService.UpdateAsync(user);
                    _logger.LogInformation($"Usuário atualizado: {user.DisplayName}");
                }
            }

            _logger.LogInformation("Migração de usuários concluída.");
        }
        #endregion

        #region REPORTS
        private async Task MigrateReportsAsync()
        {
            _logger.LogInformation("Migrando coleção: reports...");

            var reportsCollection = _firestore.Collection("reports");
            var snapshot = await reportsCollection.GetSnapshotAsync();

            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();
                var firebaseId = doc.Id;

                var existingReport = await _reportService.GetByFirebaseIdAsync(firebaseId);

                var report = existingReport ?? new Report { Id = existingReport?.Id ?? Guid.NewGuid() };

                report.FirebaseId = firebaseId;
                report.Category = data.GetValueOrDefault("category", "outros")?.ToString() ?? "outros";
                report.Title = data.GetValueOrDefault("title", string.Empty)?.ToString() ?? "";
                report.Description = data.GetValueOrDefault("description", string.Empty)?.ToString() ?? "";
                report.Status = data.GetValueOrDefault("status", "pending")?.ToString() ?? "pending";
                report.IsAnonymous = Convert.ToBoolean(data.GetValueOrDefault("isAnonymous", false));
                report.ImageUrl1 = ExtractFirstImageUrl(data);
                report.CreatedAt = ParseDate(data.GetValueOrDefault("createdAt"));
                report.UpdatedAt = ParseDate(data.GetValueOrDefault("updatedAt"));
                report.ResolvedAt = ParseDate(data.GetValueOrDefault("resolvedAt"));

                // Localização
                if (data.TryGetValue("location", out var locationObj) && locationObj is Dictionary<string, object> loc)
                {
                    report.Location ??= new ReportLocation { Id = Guid.NewGuid() };

                    report.Location.Address = loc.GetValueOrDefault("address", "")?.ToString() ?? "";
                    report.Location.Latitude = Convert.ToDecimal(loc.GetValueOrDefault("latitude", 0));
                    report.Location.Longitude = Convert.ToDecimal(loc.GetValueOrDefault("longitude", 0));
                    report.Location.PostalCode = loc.GetValueOrDefault("postalCode", "")?.ToString() ?? "";
                }

                if (existingReport == null)
                {
                    await _reportService.CreateAsync(report);
                    _logger.LogInformation($"Report criado: {report.Title}");
                }
                else
                {
                    await _reportService.UpdateAsync(report);
                    _logger.LogInformation($"Report atualizado: {report.Title}");
                }
            }

            _logger.LogInformation("Migração de reports concluída.");
        }
        #endregion

        #region HELPERS
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
        #endregion
    }
}