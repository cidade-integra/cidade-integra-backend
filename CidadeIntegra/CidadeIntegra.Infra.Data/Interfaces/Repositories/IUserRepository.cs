using CidadeIntegra.Domain.Entities;

namespace CidadeIntegra.Application.Interfaces.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
    }
}