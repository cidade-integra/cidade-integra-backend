using CidadeIntegra.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CidadeIntegra.Infra.Data.EntityConfiguration
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("Reports");

            // Chave primária
            builder.HasKey(r => r.Id);

            // Firebase ID
            builder.HasIndex(u => u.FirebaseId).IsUnique();
            builder.Property(u => u.FirebaseId).HasMaxLength(100).IsRequired();

            // Relacionamento com User
            builder.HasOne(r => r.User)
                   .WithMany(u => u.Reports)
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Relacionamento 1:1 com Location
            builder.HasOne(r => r.Location)
                   .WithOne(l => l.Report)
                   .HasForeignKey<ReportLocation>(l => l.ReportId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Relacionamento 1:N com Comments
            builder.HasMany(r => r.Comments)
                   .WithOne(c => c.Report)
                   .HasForeignKey(c => c.ReportId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Relação N:N (UserSavedReports)
            builder.HasMany(r => r.SavedByUsers)
                   .WithOne(sr => sr.Report)
                   .HasForeignKey(sr => sr.ReportId)
                   .OnDelete(DeleteBehavior.NoAction);

            // Propriedades adicionais
            builder.Property(r => r.Category)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(r => r.Title)
                   .HasMaxLength(120)
                   .IsRequired();

            builder.Property(r => r.Status)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(r => r.ImageUrl1)
                   .HasMaxLength(500);

            builder.Property(r => r.ImageUrl2)
                   .HasMaxLength(500);

            // Datas
            builder.Property(r => r.CreatedAt)
                   .HasColumnType("datetimeoffset");

            builder.Property(r => r.UpdatedAt)
                   .HasColumnType("datetimeoffset");

            builder.Property(r => r.ResolvedAt)
                   .HasColumnType("datetimeoffset");
        }
    }
}