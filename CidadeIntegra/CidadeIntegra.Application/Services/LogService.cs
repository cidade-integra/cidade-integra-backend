using CidadeIntegra.Application.Interfaces.Services;
using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Infra.Data.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CidadeIntegra.Application.Services
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _repository;
        private readonly ILogger<LogService> _logger;

        public LogService(ILogRepository repository, ILogger<LogService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task CreateLogAsync(Log log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            try
            {
                await _repository.AddAsync(log);
                await _repository.SaveChangesAsync();

                _logger.LogInformation("Log gravado com sucesso: {Message}", log.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gravar log no banco. Nível: {Level}, Mensagem: {Message}", log.Level, log.Message);
                Console.WriteLine($"Falha ao gravar log no banco: {log.Message}");
            }
        }
    }
}