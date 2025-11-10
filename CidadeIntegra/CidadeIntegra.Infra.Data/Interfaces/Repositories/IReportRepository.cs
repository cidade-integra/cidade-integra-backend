using CidadeIntegra.Domain.Entities;

namespace CidadeIntegra.Application.Interfaces.Repositories
{
    public interface IReportRepository : IGenericRepository<Report>
    {
        Task<IEnumerable<Report>> GetPendingReportsAsync();
    }
}