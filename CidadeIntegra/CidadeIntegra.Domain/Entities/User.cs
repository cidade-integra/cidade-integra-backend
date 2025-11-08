using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("Users")]
    public class User
    {
        #region Identificação
        [Key]
        public Guid Id { get; set; } //uid

        [MaxLength(100)]
        public string FirebaseId { get; set; } = string.Empty;
        #endregion

        #region Informações Pessoais
        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? PhotoUrl { get; set; }

        [MaxLength(100)]
        public string? Region { get; set; }
        #endregion

        #region Configurações e Status
        public int ReportCount { get; set; }

        [Required, MaxLength(50)]
        public string Role { get; set; } = "user";

        public int Score { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "active";

        public bool Verified { get; set; }
        #endregion

        #region Datas
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset LastLoginAt { get; set; }
        #endregion

        #region Navegações
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<UserSavedReport> SavedReports { get; set; } = new List<UserSavedReport>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        #endregion

        #region Métodos de Migração
        public static User FromFirestore(DocumentSnapshot doc)
        {
            var data = doc.ToDictionary();
            return new User
            {
                Id = Guid.NewGuid(),
                FirebaseId = doc.Id,
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
        }

        public void UpdateFromFirestore(DocumentSnapshot doc)
        {
            var data = doc.ToDictionary();
            DisplayName = data.GetValueOrDefault("displayName", string.Empty)?.ToString() ?? DisplayName;
            Email = data.GetValueOrDefault("email", string.Empty)?.ToString() ?? Email;
            PhotoUrl = data.GetValueOrDefault("photoURL", string.Empty)?.ToString() ?? PhotoUrl;
            Region = data.GetValueOrDefault("region", string.Empty)?.ToString() ?? Region;
            Role = data.GetValueOrDefault("role", "user")?.ToString() ?? Role;
            Status = data.GetValueOrDefault("status", "active")?.ToString() ?? Status;
            Score = Convert.ToInt32(data.GetValueOrDefault("score", Score));
            ReportCount = Convert.ToInt32(data.GetValueOrDefault("reportCount", ReportCount));
            Verified = Convert.ToBoolean(data.GetValueOrDefault("verified", Verified));
            CreatedAt = ParseDate(data.GetValueOrDefault("createdAt"));
            LastLoginAt = ParseDate(data.GetValueOrDefault("lastLoginAt"));
        }

        private static DateTimeOffset ParseDate(object? dateObj)
        {
            if (dateObj is Timestamp ts)
                return ts.ToDateTimeOffset();

            if (DateTimeOffset.TryParse(dateObj?.ToString(), out var parsed))
                return parsed;

            return DateTimeOffset.UtcNow;
        }
        #endregion
    }
}