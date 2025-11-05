using CidadeIntegra.Domain.Entities;

namespace CidadeIntegra.Application.Interfaces.Services
{
    public interface ILogService
    {
        Task CreateLogAsync(Log log);
    }
}
