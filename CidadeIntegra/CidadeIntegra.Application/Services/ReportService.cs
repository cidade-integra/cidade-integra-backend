using CidadeIntegra.Application.Interfaces.Repositories;
using CidadeIntegra.Application.Interfaces.Services;
using CidadeIntegra.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CidadeIntegra.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IReportRepository reportRepository, ILogger<ReportService> logger)
        {
            _reportRepository = reportRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Report>> GetPendingReportsAsync()
        {
            try
            {
                _logger.LogInformation("Buscando denúncias pendentes...");
                var reports = await _reportRepository.GetPendingReportsAsync();
                _logger.LogInformation("Busca concluída. Total de denúncias pendentes: {Count}", reports?.Count() ?? 0);

                return reports;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar denúncias pendentes");
                throw;
            }
        }

        public async Task CreateAsync(Report report)
        {
            try
            {
                _logger.LogInformation("Criando nova denúncia com FirebaseId: {FirebaseId}", report.FirebaseId);

                await _reportRepository.AddAsync(report);
                await _reportRepository.SaveChangesAsync();

                _logger.LogInformation("Denúncia criada com sucesso: {ReportId}", report.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar denúncia com FirebaseId: {FirebaseId}", report.FirebaseId);
                throw;
            }
        }

        public async Task<Report?> GetByFirebaseIdAsync(string firebaseId)
        {
            try
            {
                _logger.LogInformation("Buscando denúncia por FirebaseId: {FirebaseId}", firebaseId);
                var report = await _reportRepository.GetByFirebaseIdAsync(firebaseId);

                if (report == null)
                {
                    _logger.LogWarning("Nenhuma denúncia encontrada para FirebaseId: {FirebaseId}", firebaseId);
                }

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar denúncia por FirebaseId: {FirebaseId}", firebaseId);
                throw;
            }
        }

        public async Task UpdateAsync(Report report)
        {
            try
            {
                _logger.LogInformation("Atualizando denúncia com ID: {ReportId}", report.Id);
                await _reportRepository.UpdateAsync(report);
                _logger.LogInformation("Denúncia atualizada com sucesso: {ReportId}", report.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar denúncia com ID: {ReportId}", report.Id);
                throw;
            }
        }
    }
}