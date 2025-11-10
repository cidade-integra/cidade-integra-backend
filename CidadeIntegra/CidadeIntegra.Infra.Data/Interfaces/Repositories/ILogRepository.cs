using CidadeIntegra.Domain.Entities;

namespace CidadeIntegra.Infra.Data.Interfaces.Repositories
{
    public interface ILogRepository
    {
        Task AddAsync(Log log);
        Task SaveChangesAsync();
    }
}
