using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CidadeIntegra.Domain.Entities
{
    [Table("ReportLocations")]
    public class ReportLocation
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Report))]
        public Guid ReportId { get; set; }

        [Required, MaxLength(255)]
        public string Address { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        [Required, MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        public Report Report { get; set; } = null!;
    }
}
