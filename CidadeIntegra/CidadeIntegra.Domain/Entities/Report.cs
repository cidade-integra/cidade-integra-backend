using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("Reports")]
    public class Report
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [Required, MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public bool IsAnonymous { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "pending";

        [MaxLength(500)]
        public string? ImageUrl1 { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl2 { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public DateTimeOffset? ResolvedAt { get; set; }

        // Navegações
        public User User { get; set; } = null!;
        public ReportLocation Location { get; set; } = null!;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<UserSavedReport> SavedByUsers { get; set; } = new List<UserSavedReport>();
    }
}