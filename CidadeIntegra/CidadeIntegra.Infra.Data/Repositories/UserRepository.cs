using CidadeIntegra.Application.Interfaces.Repositories;
using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace CidadeIntegra.Infra.Data.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}