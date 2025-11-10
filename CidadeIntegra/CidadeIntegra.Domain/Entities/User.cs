using CidadeIntegra.Domain.Validation;
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
        public Guid Id { get; private set; }

        [MaxLength(100)]
        public string FirebaseId { get; private set; } = string.Empty;
        #endregion

        #region Informações Pessoais
        [Required, MaxLength(100)]
        public string DisplayName { get; private set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; private set; } = string.Empty;

        [MaxLength(255)]
        public string? PhotoUrl { get; private set; }

        [MaxLength(100)]
        public string? Region { get; private set; }
        #endregion

        #region Configurações e Status
        public int ReportCount { get; private set; }

        [Required, MaxLength(50)]
        public string Role { get; private set; } = "user";

        public int Score { get; private set; }

        [Required, MaxLength(50)]
        public string Status { get; private set; } = "active";

        public bool Verified { get; private set; }
        #endregion

        #region Datas
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset LastLoginAt { get; private set; }
        #endregion

        #region Navegações
        public ICollection<Report> Reports { get; private set; } = new List<Report>();
        public ICollection<UserSavedReport> SavedReports { get; private set; } = new List<UserSavedReport>();
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
        #endregion

        #region Construtor
        protected User() { } // Necessário para EF

        public User(Guid id, string displayName, string email, string? photoUrl, string? region, string role, string status, DateTimeOffset createdAt)
        {
            DomainExceptionValidation.When(id == Guid.Empty, "Invalid Id.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(displayName), "DisplayName is required.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email), "Email is required and must be valid.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(role), "Role is required.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(status), "Status is required.");
            DomainExceptionValidation.When(createdAt == default, "CreatedAt must be set.");

            Id = id;
            DisplayName = displayName.Trim();
            Email = email.Trim().ToLowerInvariant();
            PhotoUrl = photoUrl?.Trim();
            Region = region?.Trim();
            Role = role.Trim().ToLowerInvariant();
            Status = status.Trim().ToLowerInvariant();
            CreatedAt = createdAt;
            LastLoginAt = DateTimeOffset.UtcNow;
            ReportCount = 0;
            Score = 0;
            Verified = false;
        }
        #endregion

        /*#region Migração Firestore
        public static User FromFirestore(DocumentSnapshot doc)
        {
            var data = doc.ToDictionary();

            return new User
            {
                Id = Guid.NewGuid(),
                FirebaseId = doc.Id,
                DisplayName = data.GetValueOrDefault("displayName", string.Empty)?.ToString() ?? "",
                Email = data.GetValueOrDefault("email", string.Empty)?.ToString() ?? "",
                PhotoUrl = data.GetValueOrDefault("photoURL", string.Empty)?.ToString(),
                Region = data.GetValueOrDefault("region", string.Empty)?.ToString(),
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

            DisplayName = data.GetValueOrDefault("displayName", DisplayName)?.ToString() ?? DisplayName;
            Email = data.GetValueOrDefault("email", Email)?.ToString() ?? Email;
            PhotoUrl = data.GetValueOrDefault("photoURL", PhotoUrl)?.ToString() ?? PhotoUrl;
            Region = data.GetValueOrDefault("region", Region)?.ToString() ?? Region;
            Role = data.GetValueOrDefault("role", Role)?.ToString() ?? Role;
            Status = data.GetValueOrDefault("status", Status)?.ToString() ?? Status;
            Score = Convert.ToInt32(data.GetValueOrDefault("score", Score));
            ReportCount = Convert.ToInt32(data.GetValueOrDefault("reportCount", ReportCount));
            Verified = Convert.ToBoolean(data.GetValueOrDefault("verified", Verified));
            CreatedAt = ParseDate(data.GetValueOrDefault("createdAt"));
            LastLoginAt = ParseDate(data.GetValueOrDefault("lastLoginAt"));

            Validate();
        }

        private static DateTimeOffset ParseDate(object? dateObj)
        {
            if (dateObj is Timestamp ts)
                return ts.ToDateTimeOffset();

            if (DateTimeOffset.TryParse(dateObj?.ToString(), out var parsed))
                return parsed;

            return DateTimeOffset.UtcNow;
        }
        #endregion*/

        #region Migração Firestore

        public static User FromDictionary(Dictionary<string, object> data, string? firebaseId = null)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                FirebaseId = firebaseId ?? string.Empty,
                DisplayName = data.GetValueOrDefault("displayName", string.Empty)?.ToString() ?? "",
                Email = data.GetValueOrDefault("email", string.Empty)?.ToString() ?? "",
                PhotoUrl = data.GetValueOrDefault("photoURL", string.Empty)?.ToString(),
                Region = data.GetValueOrDefault("region", string.Empty)?.ToString(),
                Role = data.GetValueOrDefault("role", "user")?.ToString() ?? "user",
                Status = data.GetValueOrDefault("status", "active")?.ToString() ?? "active",
                Score = Convert.ToInt32(data.GetValueOrDefault("score", 0)),
                ReportCount = Convert.ToInt32(data.GetValueOrDefault("reportCount", 0)),
                Verified = Convert.ToBoolean(data.GetValueOrDefault("verified", false)),
                CreatedAt = ParseDate(data.GetValueOrDefault("createdAt")),
                LastLoginAt = ParseDate(data.GetValueOrDefault("lastLoginAt"))
            };
        }

        public static User FromFirestore(DocumentSnapshot doc)
        {
            return FromDictionary(doc.ToDictionary(), doc.Id);
        }

        public void UpdateFromDictionary(Dictionary<string, object> data)
        {
            DisplayName = data.GetValueOrDefault("displayName", DisplayName)?.ToString() ?? DisplayName;
            Email = data.GetValueOrDefault("email", Email)?.ToString() ?? Email;
            PhotoUrl = data.GetValueOrDefault("photoURL", PhotoUrl)?.ToString() ?? PhotoUrl;
            Region = data.GetValueOrDefault("region", Region)?.ToString() ?? Region;
            Role = data.GetValueOrDefault("role", Role)?.ToString() ?? Role;
            Status = data.GetValueOrDefault("status", Status)?.ToString() ?? Status;
            Score = Convert.ToInt32(data.GetValueOrDefault("score", Score));
            ReportCount = Convert.ToInt32(data.GetValueOrDefault("reportCount", ReportCount));
            Verified = Convert.ToBoolean(data.GetValueOrDefault("verified", Verified));
            CreatedAt = ParseDate(data.GetValueOrDefault("createdAt"));
            LastLoginAt = ParseDate(data.GetValueOrDefault("lastLoginAt"));

            Validate();
        }

        public void UpdateFromFirestore(DocumentSnapshot doc)
        {
            UpdateFromDictionary(doc.ToDictionary());
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


        #region Validação
        public void Validate()
        {
            var context = new ValidationContext(this);
            Validator.ValidateObject(this, context, validateAllProperties: true);

            if (string.IsNullOrWhiteSpace(DisplayName))
                throw new ValidationException("DisplayName cannot be empty.");

            if (!new EmailAddressAttribute().IsValid(Email))
                throw new ValidationException("Email is invalid.");

            if (CreatedAt > DateTimeOffset.UtcNow.AddMinutes(5))
                throw new ValidationException("CreatedAt cannot be in the future.");
        }
        #endregion
    }
}
