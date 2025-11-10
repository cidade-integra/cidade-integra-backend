using CidadeIntegra.Domain.Validation;
using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("Users")]
    public class User
    {
        #region Atributos
        [Key]
        public Guid Id { get; private set; }

        [MaxLength(100)]
        public string FirebaseId { get; private set; } = string.Empty;

        [Required, MaxLength(100)]
        public string DisplayName { get; private set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; private set; } = string.Empty;

        [MaxLength(255)]
        public string? PhotoUrl { get; private set; }

        [MaxLength(100)]
        public string? Region { get; private set; }

        public int ReportCount { get; private set; }

        [Required, MaxLength(50)]
        public string Role { get; private set; } = "user";

        public int Score { get; private set; }

        [Required, MaxLength(50)]
        public string Status { get; private set; } = "active";

        public bool Verified { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset LastLoginAt { get; private set; }
        #endregion

        #region Navegações
        public ICollection<Report> Reports { get; private set; } = new List<Report>();
        public ICollection<UserSavedReport> SavedReports { get; private set; } = new List<UserSavedReport>();
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
        #endregion

        #region Construtor
        protected User() { }

        public User(Guid id, string displayName, string email, string? photoUrl, string? region, string role, string status, DateTimeOffset createdAt)
        {
            Validate(id, displayName, email, photoUrl, region, role, status, createdAt);

            Id = id;
            DisplayName = displayName.Trim();
            Email = email.Trim().ToLowerInvariant();
            PhotoUrl = photoUrl?.Trim();
            Region = region?.Trim();
            Role = role.Trim();
            Status = status.Trim();
            CreatedAt = createdAt;
            LastLoginAt = DateTimeOffset.UtcNow;
            ReportCount = 0;
            Score = 0;
            Verified = false;
        }
        #endregion

        #region Migração Firestore
        public static User FromFirestore(DocumentSnapshot doc)
        {
            var data = doc.ToDictionary();

            var firebaseId = doc.Id;
            var displayName = data.GetValueOrDefault("displayName", string.Empty)?.ToString() ?? "";
            var email = data.GetValueOrDefault("email", string.Empty)?.ToString() ?? "";
            var photoUrl = data.GetValueOrDefault("photoURL", string.Empty)?.ToString();
            var region = data.GetValueOrDefault("region", string.Empty)?.ToString();
            var role = data.GetValueOrDefault("role", "user")?.ToString() ?? "user";
            var status = data.GetValueOrDefault("status", "active")?.ToString() ?? "active";
            var score = Convert.ToInt32(data.GetValueOrDefault("score", 0));
            var reportCount = Convert.ToInt32(data.GetValueOrDefault("reportCount", 0));
            var verified = Convert.ToBoolean(data.GetValueOrDefault("verified", false));
            var createdAt = ParseDate(data.GetValueOrDefault("createdAt"));
            var lastLoginAt = ParseDate(data.GetValueOrDefault("lastLoginAt"));

            var user = new User(
                id: Guid.NewGuid(),
                displayName: displayName,
                email: email,
                photoUrl: photoUrl,
                region: region,
                role: role,
                status: status,
                createdAt: createdAt
            );

            user.SetFirebaseId(firebaseId);
            user.SetStats(score, reportCount, verified);
            user.SetLastLoginAt(lastLoginAt);

            return user;
        }

        public void UpdateFromFirestore(DocumentSnapshot doc)
        {
            var data = doc.ToDictionary();

            var newDisplayName = data.GetValueOrDefault("displayName", DisplayName)?.ToString() ?? DisplayName;
            var newEmail = data.GetValueOrDefault("email", Email)?.ToString() ?? Email;
            var newPhotoUrl = data.GetValueOrDefault("photoURL", PhotoUrl)?.ToString();
            var newRegion = data.GetValueOrDefault("region", Region)?.ToString();
            var newRole = data.GetValueOrDefault("role", Role)?.ToString() ?? Role;
            var newStatus = data.GetValueOrDefault("status", Status)?.ToString() ?? Status;
            var score = Convert.ToInt32(data.GetValueOrDefault("score", Score));
            var reportCount = Convert.ToInt32(data.GetValueOrDefault("reportCount", ReportCount));
            var verified = Convert.ToBoolean(data.GetValueOrDefault("verified", Verified));
            var createdAt = ParseDate(data.GetValueOrDefault("createdAt"));
            var lastLoginAt = ParseDate(data.GetValueOrDefault("lastLoginAt"));

            UpdateProfile(newDisplayName, newEmail, newPhotoUrl, newRegion);
            ChangeRole(newRole);
            ChangeStatus(newStatus);
            SetStats(score, reportCount, verified);
            CreatedAt = createdAt;
            SetLastLoginAt(lastLoginAt);

            Validate(Id, DisplayName, Email, PhotoUrl, Region, Role, Status, CreatedAt);
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

        #region Comportamento / Mutadores
        public void SetFirebaseId(string firebaseId)
        {
            if (string.IsNullOrWhiteSpace(firebaseId)) return;
            FirebaseId = firebaseId.Trim();
        }

        public void UpdateProfile(string displayName, string email, string? photoUrl, string? region)
        {
            // validações locais (poderiam reutilizar Validate parcial)
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(displayName), "DisplayName is required.");
            DomainExceptionValidation.When(displayName.Length > 100, "DisplayName must have a maximum of 100 characters.");

            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(email), "Email is required.");
            DomainExceptionValidation.When(!new EmailAddressAttribute().IsValid(email), "Email must be valid.");
            DomainExceptionValidation.When(email.Length > 150, "Email must have a maximum of 150 characters.");

            if (photoUrl is not null)
                DomainExceptionValidation.When(photoUrl.Length > 255, "PhotoUrl must have a maximum of 255 characters.");

            if (region is not null)
                DomainExceptionValidation.When(region.Length > 100, "Region must have a maximum of 100 characters.");

            DisplayName = displayName.Trim();
            Email = email.Trim().ToLowerInvariant();
            PhotoUrl = photoUrl?.Trim();
            Region = region?.Trim();
        }

        public void ChangeRole(string role)
        {
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(role), "Role is required.");
            DomainExceptionValidation.When(role.Length > 50, "Role must have a maximum of 50 characters.");
            Role = role.Trim();
        }

        public void ChangeStatus(string status)
        {
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(status), "Status is required.");
            DomainExceptionValidation.When(status.Length > 50, "Status must have a maximum of 50 characters.");
            Status = status.Trim();
        }

        public void SetStats(int score, int reportCount, bool verified)
        {
            DomainExceptionValidation.When(score < 0, "Score cannot be negative.");
            DomainExceptionValidation.When(reportCount < 0, "ReportCount cannot be negative.");

            Score = score;
            ReportCount = reportCount;
            Verified = verified;
        }

        public void SetLastLoginAt(DateTimeOffset lastLoginAt)
        {
            if (lastLoginAt == default) return;
            LastLoginAt = lastLoginAt;
        }
        #endregion

        #region Validação
        public static void Validate(
            Guid id,
            string displayName,
            string email,
            string? photoUrl,
            string? region,
            string role,
            string status,
            DateTimeOffset createdAt)
        {
            DomainExceptionValidation.When(id == Guid.Empty, "Invalid Id.");

            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(displayName), "DisplayName is required.");
            DomainExceptionValidation.When(displayName.Length > 100, "DisplayName must have a maximum of 100 characters.");

            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(email), "Email is required.");
            DomainExceptionValidation.When(!new EmailAddressAttribute().IsValid(email), "Email must be valid.");
            DomainExceptionValidation.When(email.Length > 150, "Email must have a maximum of 150 characters.");

            if (!string.IsNullOrEmpty(photoUrl))
                DomainExceptionValidation.When(photoUrl.Length > 255, "PhotoUrl must have a maximum of 255 characters.");

            if (!string.IsNullOrEmpty(region))
                DomainExceptionValidation.When(region.Length > 100, "Region must have a maximum of 100 characters.");

            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(role), "Role is required.");
            DomainExceptionValidation.When(role.Length > 50, "Role must have a maximum of 50 characters.");

            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(status), "Status is required.");
            DomainExceptionValidation.When(status.Length > 50, "Status must have a maximum of 50 characters.");

            DomainExceptionValidation.When(createdAt == default, "CreatedAt must be set.");
        }
        #endregion
    }
}