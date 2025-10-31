using CidadeIntegra.Application.Interfaces.Repositories;
using CidadeIntegra.Application.Interfaces.Services;
using CidadeIntegra.Domain.Entities;

namespace CidadeIntegra.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<IEnumerable<Report>> GetPendingReportsAsync()
        {
            return await _reportRepository.GetPendingReportsAsync();
        }

        public async Task CreateAsync(Report report)
        {
            await _reportRepository.AddAsync(report);
            await _reportRepository.SaveChangesAsync();
        }

        public async Task<Report?> GetByFirebaseIdAsync(string firebaseId)
            => await _reportRepository.GetByFirebaseIdAsync(firebaseId);

        public async Task UpdateAsync(Report report)
            => await _reportRepository.UpdateAsync(report);

    }
}