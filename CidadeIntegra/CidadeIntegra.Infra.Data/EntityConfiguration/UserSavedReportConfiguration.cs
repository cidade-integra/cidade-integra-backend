using CidadeIntegra.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CidadeIntegra.Infra.Data.EntityConfiguration
{
    public class UserSavedReportConfiguration : IEntityTypeConfiguration<UserSavedReport>
    {
        public void Configure(EntityTypeBuilder<UserSavedReport> builder)
        {
            builder.ToTable("UserSavedReports");

            builder.HasKey(sr => new { sr.UserId, sr.ReportId });

            builder.HasOne(sr => sr.User)
                   .WithMany(u => u.SavedReports)
                   .HasForeignKey(sr => sr.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(sr => sr.Report)
                   .WithMany(r => r.SavedByUsers)
                   .HasForeignKey(sr => sr.ReportId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}