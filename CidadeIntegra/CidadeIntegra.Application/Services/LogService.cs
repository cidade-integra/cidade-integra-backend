using CidadeIntegra.Application.Interfaces.Services;
using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Infra.Data.Context;
using Microsoft.Extensions.Logging;

namespace CidadeIntegra.Application.Services
{
    public class LogService : ILogService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LogService> _logger;

        public LogService(AppDbContext context, ILogger<LogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateLogAsync(Log log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            try
            {
                _context.Logs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Log gravado com sucesso: {Message}", log.Message);
            }
            catch (Exception ex)
            {
                // loga o erro sem interromper o fluxo principal
                _logger.LogError(ex, "Erro ao gravar log no banco. Nível: {Level}, Mensagem: {Message}", log.Level, log.Message);

                // tentar salvar em arquivo ou console alternativo
                Console.WriteLine($"Falha ao gravar log no banco: {log.Message}");
            }
        }
    }
}