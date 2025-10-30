using CidadeIntegra.Application.Interfaces.Services;
using CidadeIntegra.Domain.Entities;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CidadeIntegra.Application.Services
{
    public class MigrationService : IMigrationService
    {
        private readonly IUserService _userService;
        private readonly IReportService _reportService;
        private readonly ILogger<MigrationService> _logger;
        private readonly FirestoreDb _firestore;

        private readonly Dictionary<string, Guid> _userMap = new();

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

            var usersSnapshot = await _firestore.Collection("users").GetSnapshotAsync();

            foreach (var doc in usersSnapshot.Documents)
            {
                var data = doc.ToDictionary();
                var firestoreUid = doc.Id;

                var userGuid = Guid.NewGuid();
                _userMap[firestoreUid] = userGuid;

                var user = new User
                {
                    Id = userGuid,
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
                else
                {
                    _userMap[firestoreUid] = existing.Id;
                }
            }

            _logger.LogInformation("Migração de usuários concluída.");
        }
        #endregion

        #region REPORTS
        private async Task MigrateReportsAsync()
        {
            _logger.LogInformation("Migrando coleção: reports...");

            var reportsSnapshot = await _firestore.Collection("reports").GetSnapshotAsync();

            foreach (var doc in reportsSnapshot.Documents)
            {
                var data = doc.ToDictionary();
                var firestoreUserId = data.GetValueOrDefault("userId", null)?.ToString();

                // Relaciona com o usuário correspondente
                if (firestoreUserId == null || !_userMap.ContainsKey(firestoreUserId))
                {
                    _logger.LogWarning($"Usuário não encontrado para report {doc.Id}, ignorando...");
                    continue;
                }

                var report = new Report
                {
                    Id = Guid.NewGuid(),
                    UserId = _userMap[firestoreUserId],
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