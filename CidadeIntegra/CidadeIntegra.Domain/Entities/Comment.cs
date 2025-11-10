using CidadeIntegra.Domain.Validation;
using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("Comments")]
    public class Comment
    {
        #region Atributos
        [Key]
        public Guid Id { get; private set; }

        [ForeignKey(nameof(Report))]
        public Guid ReportId { get; private set; }

        [ForeignKey(nameof(Author))]
        public Guid AuthorId { get; private set; }

        [Required, MaxLength(50)]
        public string AvatarColor { get; private set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Message { get; private set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Role { get; private set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; private set; }

        public Report Report { get; private set; } = null!;
        public User Author { get; private set; } = null!;
        #endregion

        #region Construtores
        protected Comment() { } // EF

        private Comment(Guid id, Guid reportId, Guid authorId, string avatarColor, string message, string role, DateTimeOffset createdAt)
        {
            ValidateParameters(id, reportId, authorId, avatarColor, message, role, createdAt);

            Id = id;
            ReportId = reportId;
            AuthorId = authorId;
            AvatarColor = avatarColor.Trim();
            Message = message.Trim();
            Role = role.Trim();
            CreatedAt = createdAt;
        }
        #endregion

        #region Fábrica
        public static Comment Create(Guid reportId, Guid authorId, string avatarColor, string message, string role)
        {
            return new Comment(
                Guid.NewGuid(),
                reportId,
                authorId,
                avatarColor,
                message,
                role,
                DateTimeOffset.UtcNow
            );
        }
        #endregion

        #region Fabricação/Atualização Firestore
        public static Comment FromFirestore(DocumentSnapshot doc, Guid reportId, Dictionary<string, Guid> userMap)
        {
            var data = doc.ToDictionary();

            // resolve o AuthorId usando o FirebaseId do autor
            Guid authorId = Guid.Empty;
            var authorFirebaseId = data.GetValueOrDefault("authorId")?.ToString();
            if (authorFirebaseId != null && userMap.TryGetValue(authorFirebaseId, out var mappedUserId))
            {
                authorId = mappedUserId;
            }

            return new Comment(
                Guid.NewGuid(),
                reportId,
                authorId,
                data.GetValueOrDefault("avatarColor", string.Empty)?.ToString() ?? string.Empty,
                data.GetValueOrDefault("message", string.Empty)?.ToString() ?? string.Empty,
                data.GetValueOrDefault("role", string.Empty)?.ToString() ?? string.Empty,
                ParseDate(data.GetValueOrDefault("createdAt"))
            );
        }

        public void UpdateFromFirestore(DocumentSnapshot doc, Dictionary<string, Guid> userMap)
        {
            var data = doc.ToDictionary();

            var authorFirebaseId = data.GetValueOrDefault("authorId")?.ToString();
            if (authorFirebaseId != null && userMap.TryGetValue(authorFirebaseId, out var mappedUserId))
            {
                AuthorId = mappedUserId;
            }

            AvatarColor = data.GetValueOrDefault("avatarColor", AvatarColor)?.ToString() ?? AvatarColor;
            Message = data.GetValueOrDefault("message", Message)?.ToString() ?? Message;
            Role = data.GetValueOrDefault("role", Role)?.ToString() ?? Role;
            CreatedAt = ParseDate(data.GetValueOrDefault("createdAt"));

            ValidateParameters(Id, ReportId, AuthorId, AvatarColor, Message, Role, CreatedAt);
        }
        #endregion

        #region Validação
        private void ValidateParameters(Guid id, Guid reportId, Guid authorId, string avatarColor, string message, string role, DateTimeOffset createdAt)
        {
            DomainExceptionValidation.When(id == Guid.Empty, "Id cannot be empty.");
            DomainExceptionValidation.When(reportId == Guid.Empty, "ReportId cannot be empty.");
            DomainExceptionValidation.When(authorId == Guid.Empty, "AuthorId cannot be empty.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(avatarColor), "AvatarColor is required.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(message), "Message is required.");
            DomainExceptionValidation.When(string.IsNullOrWhiteSpace(role), "Role is required.");
            DomainExceptionValidation.When(avatarColor.Length > 50, "AvatarColor length cannot exceed 50 characters.");
            DomainExceptionValidation.When(message.Length > 500, "Message length cannot exceed 500 characters.");
            DomainExceptionValidation.When(role.Length > 50, "Role length cannot exceed 50 characters.");
            DomainExceptionValidation.When(createdAt > DateTimeOffset.UtcNow.AddMinutes(5), "CreatedAt cannot be a future date.");
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
        #endregion
    }
}