using CidadeIntegra.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CidadeIntegra.Infra.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        // colocar as entidades que serão mapeadas para tabelas no sql (users, reports, comments, etc.)
        public DbSet<User> Users => Set<User>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<ReportLocation> ReportLocations => Set<ReportLocation>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<UserSavedReport> UserSavedReports => Set<UserSavedReport>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}