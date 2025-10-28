using CidadeIntegra.Application.Interfaces.Repositories;
using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CidadeIntegra.Infra.Data.Repositories
{
    public class ReportRepository : GenericRepository<Report>, IReportRepository
    {
        public ReportRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Report>> GetPendingReportsAsync()
        {
            return await _dbSet
                .Where(r => r.Status == "pending")
                .Include(r => r.User)
                .Include(r => r.Location)
                .Include(r => r.Comments)
                .ToListAsync();
        }
    }
}