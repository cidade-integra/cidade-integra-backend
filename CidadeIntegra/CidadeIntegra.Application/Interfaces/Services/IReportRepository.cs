using CidadeIntegra.Domain.Entities;

namespace CidadeIntegra.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<IEnumerable<Report>> GetPendingReportsAsync();
    }
}