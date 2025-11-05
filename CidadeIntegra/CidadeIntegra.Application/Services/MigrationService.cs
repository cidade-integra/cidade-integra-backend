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
        private readonly ILogService _logService;
        private readonly ILogger<MigrationService> _logger;
        private readonly FirestoreDb _firestore;

        // Mapeia FirebaseId -> Guid do usuário salvo no SQL
        private readonly Dictionary<string, Guid> _userMap = new();

        public MigrationService(
            IUserService userService,
            IReportService reportService,
            ILogService logService,
            ILogger<MigrationService> logger,
            IConfiguration configuration)
        {
            _userService = userService;
            _reportService = reportService;
            _logService = logService;
            _logger = logger;

            var projectId = configuration["Firebase:ProjectId"];
            var credentialsPath = configuration["Firebase:ServiceAccountPath"];
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

            _firestore = FirestoreDb.Create(projectId);
        }

        public async Task RunMigrationAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando processo de migração...");
                await LogAsync("Information", "Iniciando processo de migração...");

                await MigrateUsersAsync();
                await MigrateReportsAsync();

                _logger.LogInformation("Migração concluída com sucesso!");
                await LogAsync("Information", "Migração concluída com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o processo de migração.");
                await LogAsync("Error", "Erro durante o processo de migração.", ex);
                throw;
            }
        }

        #region USERS
        private async Task MigrateUsersAsync()
        {
            _logger.LogInformation("Migrando coleção: users...");
            await LogAsync("Information", "Migrando coleção: users...");

            var usersCollection = _firestore.Collection("users");
            var snapshot = await usersCollection.GetSnapshotAsync();

            int createdCount = 0;
            int updatedCount = 0;

            foreach (var doc in snapshot.Documents)
            {
                try
                {
                    var data = doc.ToDictionary();
                    var firebaseId = doc.Id;

                    var existingUser = await _userService.GetByFirebaseIdAsync(firebaseId);
                    var user = existingUser ?? new User { Id = existingUser?.Id ?? Guid.NewGuid() };

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
                        createdCount++;
                        _logger.LogInformation($"Usuário criado: {user.DisplayName}");
                        await LogAsync("Information", $"Usuário criado: {user.Email}");
                    }
                    else
                    {
                        await _userService.UpdateAsync(user);
                        updatedCount++;
                        _logger.LogInformation($"Usuário atualizado: {user.DisplayName}");
                        await LogAsync("Information", $"Usuário atualizado: {user.Email}");
                    }

                    _userMap[firebaseId] = user.Id;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao migrar usuário {DocId}", doc.Id);
                    await LogAsync("Error", $"Erro ao migrar usuário {doc.Id}", ex);
                }
            }

            _logger.LogInformation("Migração de usuários concluída. {CreatedCount} criados, {UpdatedCount} atualizados, total {Total}.",
                createdCount, updatedCount, createdCount + updatedCount);
            await LogAsync("Information", $"Migração de usuários concluída: {createdCount} criados, {updatedCount} atualizados.");
        }
        #endregion

        #region REPORTS
        private async Task MigrateReportsAsync()
        {
            _logger.LogInformation("Migrando coleção: reports...");
            await LogAsync("Information", "Migrando coleção: reports...");

            var reportsCollection = _firestore.Collection("reports");
            var snapshot = await reportsCollection.GetSnapshotAsync();

            int createdCount = 0;
            int updatedCount = 0;
            int skippedCount = 0;

            foreach (var doc in snapshot.Documents)
            {
                try
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

                    var userIdFirestore = data.GetValueOrDefault("userId", null)?.ToString();
                    if (userIdFirestore != null && _userMap.TryGetValue(userIdFirestore, out var userId))
                    {
                        report.UserId = userId;
                    }
                    else
                    {
                        skippedCount++;
                        string msg = $"Usuário não encontrado para o report {report.Title}";
                        _logger.LogWarning(msg);
                        await LogAsync("Warning", msg);
                        continue;
                    }

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
                        createdCount++;
                        _logger.LogInformation($"Report criado: {report.Title}");
                        await LogAsync("Information", $"Report criado: {report.Title}");
                    }
                    else
                    {
                        await _reportService.UpdateAsync(report);
                        updatedCount++;
                        _logger.LogInformation($"Report atualizado: {report.Title}");
                        await LogAsync("Information", $"Report atualizado: {report.Title}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao migrar report {DocId}", doc.Id);
                    await LogAsync("Error", $"Erro ao migrar report {doc.Id}", ex);
                }
            }

            _logger.LogInformation("Migração de reports concluída. {CreatedCount} criados, {UpdatedCount} atualizados, {SkippedCount} ignorados, total {Total}.",
                createdCount, updatedCount, skippedCount, createdCount + updatedCount);
            await LogAsync("Information", $"Migração de reports concluída: {createdCount} criados, {updatedCount} atualizados, {skippedCount} ignorados.");
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

        private async Task LogAsync(string level, string message, Exception? ex = null)
        {
            try
            {
                await _logService.CreateLogAsync(new Log
                {
                    Level = level,
                    Message = message,
                    Exception = ex?.ToString()
                });
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Falha ao gravar log no banco: {Message}", message);
                Console.WriteLine(message);
            }
        }
        #endregion
    }
}