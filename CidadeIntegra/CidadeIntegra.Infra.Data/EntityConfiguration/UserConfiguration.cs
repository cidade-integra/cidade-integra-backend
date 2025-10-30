using CidadeIntegra.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CidadeIntegra.Infra.Data.EntityConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.DisplayName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(u => u.PhotoUrl).HasMaxLength(255);
            builder.Property(u => u.Region).HasMaxLength(100);
            builder.Property(u => u.Role).HasMaxLength(50);
            builder.Property(u => u.Status).HasMaxLength(50);

            // Relações
            builder.HasMany(u => u.Reports)
                   .WithOne(r => r.User)
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(u => u.SavedReports)
                   .WithOne(sr => sr.User)
                   .HasForeignKey(sr => sr.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(u => u.Comments)
                   .WithOne(c => c.Author)
                   .HasForeignKey(c => c.AuthorId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}