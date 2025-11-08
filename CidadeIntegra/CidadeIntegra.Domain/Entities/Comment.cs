using Google.Cloud.Firestore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("Comments")]
    public class Comment
    {
        #region Propriedades
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

        public DateTimeOffset CreatedAt { get; set; }
        #endregion

        #region Navegações
        public Report Report { get; private set; } = null!;
        public User Author { get; private set; } = null!;
        #endregion

        #region Fabricação/Atualização Firestore
        public static Comment FromFirestore(DocumentSnapshot doc, Guid reportId, Dictionary<string, Guid> userMap)
        {
            var data = doc.ToDictionary();

            // Resolve o AuthorId usando o FirebaseId do autor
            Guid authorId = Guid.Empty;
            var authorFirebaseId = data.GetValueOrDefault("authorId")?.ToString();
            if (authorFirebaseId != null && userMap.TryGetValue(authorFirebaseId, out var mappedUserId))
            {
                authorId = mappedUserId;
            }

            return new Comment
            {
                Id = Guid.NewGuid(),
                ReportId = reportId,
                AuthorId = authorId,
                AvatarColor = data.GetValueOrDefault("avatarColor", string.Empty)?.ToString() ?? string.Empty,
                Message = data.GetValueOrDefault("message", string.Empty)?.ToString() ?? string.Empty,
                Role = data.GetValueOrDefault("role", string.Empty)?.ToString() ?? string.Empty,
                CreatedAt = ParseDate(data.GetValueOrDefault("createdAt"))
            };
        }

        public void UpdateFromFirestore(DocumentSnapshot doc, Dictionary<string, Guid> userMap)
        {
            var data = doc.ToDictionary();

            // Atualiza o AuthorId se possível
            var authorFirebaseId = data.GetValueOrDefault("authorId")?.ToString();
            if (authorFirebaseId != null && userMap.TryGetValue(authorFirebaseId, out var mappedUserId))
            {
                AuthorId = mappedUserId;
            }

            AvatarColor = data.GetValueOrDefault("avatarColor", string.Empty)?.ToString() ?? AvatarColor;
            Message = data.GetValueOrDefault("message", string.Empty)?.ToString() ?? Message;
            Role = data.GetValueOrDefault("role", string.Empty)?.ToString() ?? Role;
            CreatedAt = ParseDate(data.GetValueOrDefault("createdAt"));
        }
        #endregion

        #region Validação
        public void Validate()
        {
            var context = new ValidationContext(this);
            Validator.ValidateObject(this, context, validateAllProperties: true);

            if (string.IsNullOrWhiteSpace(Message))
                throw new ValidationException("Message cannot be empty.");

            if (string.IsNullOrWhiteSpace(Role))
                throw new ValidationException("Role cannot be empty.");
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