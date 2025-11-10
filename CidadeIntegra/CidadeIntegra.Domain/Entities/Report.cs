using CidadeIntegra.Domain.Validation;
using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("Reports")]
    public class Report
    {
        #region Atributos
        [Key]
        public Guid Id { get; private set; }

        [MaxLength(100)]
        public string FirebaseId { get; private set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; private set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Category { get; private set; } = string.Empty;

        [Required, StringLength(120, MinimumLength = 3)]
        public string Title { get; private set; } = string.Empty;

        [Required, StringLength(2000, MinimumLength = 5)]
        public string Description { get; private set; } = string.Empty;

        public bool IsAnonymous { get; private set; }

        [Required, StringLength(50)]
        public string Status { get; private set; } = "pending";

        [MaxLength(500)]
        public string? ImageUrl1 { get; private set; }

        [MaxLength(500)]
        public string? ImageUrl2 { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }
        public DateTimeOffset? ResolvedAt { get; set; }
        #endregion

        #region Navegações
        public User User { get; private set; } = null!;
        public ReportLocation Location { get; private set; } = null!;
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();
        public ICollection<UserSavedReport> SavedByUsers { get; private set; } = new List<UserSavedReport>();
        #endregion

        #region Construtores
        protected Report() { } // EF

        public Report(Guid userId, string category, string title, string description, bool isAnonymous, string status = "pending")
        {
            Validate(userId, category, title, description, status);

            Id = Guid.NewGuid();
            UserId = userId;
            Category = category.Trim();
            Title = title.Trim();
            Description = description.Trim();
            IsAnonymous = isAnonymous;
            Status = status.Trim().ToLowerInvariant();
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        #endregion

        #region Migração Firestore
        public static Report FromFirestore(DocumentSnapshot doc, Guid userId)
        {
            var data = doc.ToDictionary();

            var category = data.GetValueOrDefault("category", "outros")?.ToString() ?? "outros";
            var title = data.GetValueOrDefault("title", string.Empty)?.ToString() ?? "";
            var description = data.GetValueOrDefault("description", string.Empty)?.ToString() ?? "";
            var status = data.GetValueOrDefault("status", "pending")?.ToString() ?? "pending";
            var isAnonymous = Convert.ToBoolean(data.GetValueOrDefault("isAnonymous", false));
            var imageUrl1 = ExtractFirstImageUrl(data);
            var createdAt = ParseDate(data.GetValueOrDefault("createdAt"));
            var updatedAt = ParseDate(data.GetValueOrDefault("updatedAt"));
            var resolvedAt = ParseDateNullable(data.GetValueOrDefault("resolvedAt"));

            var report = new Report(userId, category, title, description, isAnonymous, status)
            {
                FirebaseId = doc.Id,
                ImageUrl1 = imageUrl1,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                ResolvedAt = resolvedAt
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

            return report;
        }

        public void UpdateFromFirestore(DocumentSnapshot doc, Guid userId)
        {
            var data = doc.ToDictionary();

            var category = data.GetValueOrDefault("category", Category)?.ToString() ?? Category;
            var title = data.GetValueOrDefault("title", Title)?.ToString() ?? Title;
            var description = data.GetValueOrDefault("description", Description)?.ToString() ?? Description;
            var status = data.GetValueOrDefault("status", Status)?.ToString() ?? Status;
            var isAnonymous = Convert.ToBoolean(data.GetValueOrDefault("isAnonymous", IsAnonymous));
            var imageUrl1 = data.GetValueOrDefault("imagemUrls", ImageUrl1)?.ToString() ?? ImageUrl1;
            var updatedAt = ParseDate(data.GetValueOrDefault("updatedAt"));
            var resolvedAt = ParseDateNullable(data.GetValueOrDefault("resolvedAt"));

            Validate(userId, category, title, description, status);

            UserId = userId;
            Category = category.Trim();
            Title = title.Trim();
            Description = description.Trim();
            Status = status.Trim().ToLowerInvariant();
            IsAnonymous = isAnonymous;
            ImageUrl1 = imageUrl1;
            UpdatedAt = updatedAt;
            ResolvedAt = resolvedAt;

            if (data.TryGetValue("location", out var locationObj) && locationObj is Dictionary<string, object> loc)
            {
                Location ??= new ReportLocation { Id = Guid.NewGuid() };
                Location.Address = loc.GetValueOrDefault("address", Location.Address)?.ToString() ?? Location.Address;
                Location.Latitude = Convert.ToDecimal(loc.GetValueOrDefault("latitude", Location.Latitude));
                Location.Longitude = Convert.ToDecimal(loc.GetValueOrDefault("longitude", Location.Longitude));
                Location.PostalCode = loc.GetValueOrDefault("postalCode", Location.PostalCode)?.ToString() ?? Location.PostalCode;
            }
        }
        #endregion

        #region Validação
        private void Validate(Guid userId, string category, string title, string description, string status)
        {
            DomainExceptionValidation.When(userId == Guid.Empty, "UserId must be provided.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(category), "Category is required.");
            DomainExceptionValidation.When(category.Length > 50, "Category length cannot exceed 50 characters.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(title), "Title is required.");
            DomainExceptionValidation.When(title.Length > 120, "Title length cannot exceed 120 characters.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(description), "Description is required.");
            DomainExceptionValidation.When(description.Length > 2000, "Description length cannot exceed 2000 characters.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(status), "Status is required.");
            DomainExceptionValidation.When(!IsValidStatus(status), $"Invalid status '{status}'.");
        }

        public void ValidateResolvedAt()
        {
            if (ResolvedAt.HasValue && ResolvedAt < CreatedAt)
                throw new ValidationException("ResolvedAt cannot be before CreatedAt.");
        }

        private static bool IsValidStatus(string status)
        {
            string[] validStatuses = { "pending", "review", "in progress", "resolved", "rejected" };
            return validStatuses.Contains(status?.Trim().ToLowerInvariant());
        }
        #endregion

        #region Helpers
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