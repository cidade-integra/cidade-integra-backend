using CidadeIntegra.Domain.Entities;

namespace CidadeIntegra.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
    }
}