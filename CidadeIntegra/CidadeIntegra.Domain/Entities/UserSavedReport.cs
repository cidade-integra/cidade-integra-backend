using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("UserSavedReports")]
    public class UserSavedReport
    {
        [Key, Column(Order = 0)]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey(nameof(Report))]
        public Guid ReportId { get; set; }

        public User User { get; set; } = null!;
        public Report Report { get; set; } = null!;
    }
}
