using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public Guid Id { get; set; } // uid

        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? PhotoUrl { get; set; }

        [MaxLength(100)]
        public string? Region { get; set; }

        public int ReportCount { get; set; }

        [Required, MaxLength(50)]
        public string Role { get; set; } = "user";

        public int Score { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "active";

        public bool Verified { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset LastLoginAt { get; set; }

        // Navegações
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<UserSavedReport> SavedReports { get; set; } = new List<UserSavedReport>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}