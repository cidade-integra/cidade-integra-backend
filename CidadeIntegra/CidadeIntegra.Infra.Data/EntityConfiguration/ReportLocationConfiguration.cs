using CidadeIntegra.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CidadeIntegra.Infra.Data.EntityConfiguration
{
    public class ReportLocationConfiguration : IEntityTypeConfiguration<ReportLocation>
    {
        public void Configure(EntityTypeBuilder<ReportLocation> builder)
        {
            builder.ToTable("ReportLocations");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Address)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(l => l.PostalCode)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(l => l.Latitude)
                   .HasPrecision(9, 6);

            builder.Property(l => l.Longitude)
                   .HasPrecision(9, 6);
        }
    }
}