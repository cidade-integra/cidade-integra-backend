using CidadeIntegra.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CidadeIntegra.Infra.Data.EntityConfiguration
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.AuthorName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(c => c.AvatarColor)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.Message)
                   .IsRequired();

            builder.Property(c => c.Role)
                   .IsRequired()
                   .HasMaxLength(50);
        }
    }
}