using CidadeIntegra.Domain.Validation;
using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("Reports")]
    public class Report
    {
        #region Identificação
        [Key]
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string FirebaseId { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
        #endregion

        #region Informações
        [Required, StringLength(50, MinimumLength = 3)]
        public string Category { get; set; } = string.Empty;

        [Required, StringLength(120, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(2000, MinimumLength = 5)]
        public string Description { get; set; } = string.Empty;

        public bool IsAnonymous { get; set; }

        [Required, StringLength(50)]
        public string Status { get; set; } = "pending";

        [MaxLength(500)]
        public string? ImageUrl1 { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl2 { get; set; } = string.Empty;
        #endregion

        #region Datas
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? ResolvedAt { get; set; }
        #endregion

        #region Navegações
        public User User { get; set; } = null!;
        public ReportLocation Location { get; set; } = null!;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<UserSavedReport> SavedByUsers { get; set; } = new List<UserSavedReport>();
        #endregion

        #region Construtores
        protected Report() { } // EF

        public Report(Guid userId, string category, string title, string description, bool isAnonymous, string status = "pending")
        {
            DomainExceptionValidation.When(userId == Guid.Empty, "Invalid UserId.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(category), "Category is required.");
            DomainExceptionValidation.When(category.Length > 50, "Category length cannot exceed 50 characters.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(title), "Title is required.");
            DomainExceptionValidation.When(title.Length > 120, "Title length cannot exceed 120 characters.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(description), "Description is required.");
            DomainExceptionValidation.When(description.Length > 2000, "Description length cannot exceed 2000 characters.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(status), "Status is required.");
            DomainExceptionValidation.When(!IsValidStatus(status), "Invalid status value.");

            Id = Guid.NewGuid();
            UserId = userId;
            Category = category.Trim();
            Title = title.Trim();
            Description = description.Trim();
            IsAnonymous = isAnonymous;
            Status = status.Trim().ToLowerInvariant();
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;

            Validate();
        }
        #endregion

        #region Métodos de Migração
        public static Report FromFirestore(DocumentSnapshot doc, Guid userId)
        {
            var data = doc.ToDictionary();
            var report = new Report
            {
                Id = Guid.NewGuid(),
                FirebaseId = doc.Id,
                UserId = userId,
                Category = data.GetValueOrDefault("category", "outros")?.ToString() ?? "outros",
                Title = data.GetValueOrDefault("title", string.Empty)?.ToString() ?? "",
                Description = data.GetValueOrDefault("description", string.Empty)?.ToString() ?? "",
                Status = data.GetValueOrDefault("status", "pending")?.ToString() ?? "pending",
                IsAnonymous = Convert.ToBoolean(data.GetValueOrDefault("isAnonymous", false)),
                ImageUrl1 = ExtractFirstImageUrl(data),
                CreatedAt = ParseDate(data.GetValueOrDefault("createdAt")),
                UpdatedAt = ParseDate(data.GetValueOrDefault("updatedAt")),
                ResolvedAt = ParseDateNullable(data.GetValueOrDefault("resolvedAt"))
            };

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

            report.Validate();
            return report;
        }

        public void UpdateFromFirestore(DocumentSnapshot doc, Guid userId)
        {
            var data = doc.ToDictionary();
            UserId = userId;
            Category = data.GetValueOrDefault("category", Category)?.ToString() ?? Category;
            Title = data.GetValueOrDefault("title", Title)?.ToString() ?? Title;
            Description = data.GetValueOrDefault("description", Description)?.ToString() ?? Description;
            Status = data.GetValueOrDefault("status", Status)?.ToString() ?? Status;
            IsAnonymous = Convert.ToBoolean(data.GetValueOrDefault("isAnonymous", IsAnonymous));
            ImageUrl1 = data.GetValueOrDefault("imagemUrls", ImageUrl1)?.ToString() ?? ImageUrl1;
            UpdatedAt = ParseDate(data.GetValueOrDefault("updatedAt"));
            ResolvedAt = ParseDateNullable(data.GetValueOrDefault("resolvedAt"));

            if (data.TryGetValue("location", out var locationObj) && locationObj is Dictionary<string, object> loc)
            {
                Location ??= new ReportLocation { Id = Guid.NewGuid() };
                Location.Address = loc.GetValueOrDefault("address", Location.Address)?.ToString() ?? Location.Address;
                Location.Latitude = Convert.ToDecimal(loc.GetValueOrDefault("latitude", Location.Latitude));
                Location.Longitude = Convert.ToDecimal(loc.GetValueOrDefault("longitude", Location.Longitude));
                Location.PostalCode = loc.GetValueOrDefault("postalCode", Location.PostalCode)?.ToString() ?? Location.PostalCode;
            }

            Validate();
        }
        #endregion

        #region Validation
        public void Validate()
        {
            if (UserId == Guid.Empty)
                throw new ValidationException("UserId must be provided.");

            if (string.IsNullOrWhiteSpace(Title))
                throw new ValidationException("Title is required.");

            if (string.IsNullOrWhiteSpace(Description))
                throw new ValidationException("Description is required.");

            if (string.IsNullOrWhiteSpace(Category))
                throw new ValidationException("Category is required.");

            if (!IsValidStatus(Status))
                throw new ValidationException($"Invalid status '{Status}'.");

            if (ResolvedAt.HasValue && ResolvedAt < CreatedAt)
                throw new ValidationException("ResolvedAt cannot be before CreatedAt.");

            if (CreatedAt == default)
                CreatedAt = DateTimeOffset.UtcNow;

            if (UpdatedAt == default)
                UpdatedAt = DateTimeOffset.UtcNow;
        }

        private static bool IsValidStatus(string status)
        {
            string[] validStatuses = { "pending", "in_progress", "resolved", "rejected" };
            return validStatuses.Contains(status?.Trim().ToLowerInvariant());
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

        private static DateTimeOffset? ParseDateNullable(object? dateObj)
        {
            if (dateObj == null) return null;
            return ParseDate(dateObj);
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
