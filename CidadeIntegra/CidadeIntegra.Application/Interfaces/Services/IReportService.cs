using CidadeIntegra.Domain.Entities;

namespace CidadeIntegra.Application.Interfaces.Services
{
    public interface IReportService
    {
        Task<IEnumerable<Report>> GetPendingReportsAsync();
        Task<Report?> GetByFirebaseIdAsync(string firebaseId);
        Task CreateAsync(Report report);
        Task UpdateAsync(Report report);
    }
}