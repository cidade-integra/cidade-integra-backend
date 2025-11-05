using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Infra.Data.Context;
using CidadeIntegra.Infra.Data.Interfaces.Repositories;

namespace CidadeIntegra.Infra.Data.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly AppDbContext _context;

        public LogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Log log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            await _context.Logs.AddAsync(log);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}