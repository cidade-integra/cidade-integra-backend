using CidadeIntegra.Application.Interfaces.Repositories;
using CidadeIntegra.Application.Interfaces.Services;
using CidadeIntegra.Application.Services;
using CidadeIntegra.Infra.Data.Context;
using CidadeIntegra.Infra.Data.Firebase;
using CidadeIntegra.Infra.Data.Interfaces.Provider;
using CidadeIntegra.Infra.Data.Interfaces.Repositories;
using CidadeIntegra.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CidadeIntegra.Infra.IoC
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureAPI(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
             options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"
            ), b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();

            services.AddScoped<IFirestoreProvider, FirestoreProvider>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IMigrationService, MigrationService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<ICommentService, CommentService>();

            return services;
        }
    }
}