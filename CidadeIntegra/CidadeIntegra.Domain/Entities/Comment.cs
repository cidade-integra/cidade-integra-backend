using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("Comments")]
    public class Comment
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Report))]
        public Guid ReportId { get; set; }

        [ForeignKey(nameof(Author))]
        public Guid AuthorId { get; set; }

        [Required, MaxLength(100)]
        public string AuthorName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string AvatarColor { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        // Navegações
        public Report Report { get; set; } = null!;
        public User Author { get; set; } = null!;
    }
}
