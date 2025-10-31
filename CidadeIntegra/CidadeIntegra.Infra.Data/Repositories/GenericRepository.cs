using CidadeIntegra.Application.Interfaces.Repositories;
using CidadeIntegra.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace CidadeIntegra.Infra.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

        public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public virtual async Task UpdateAsync(T entity)
        {
            var entry = _context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                var key = _context.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.FirstOrDefault();
                if (key != null)
                {
                    var keyValue = key.PropertyInfo?.GetValue(entity);
                    var existing = await _dbSet.FindAsync(keyValue);
                    if (existing != null)
                    {
                        _context.Entry(existing).CurrentValues.SetValues(entity);
                    }
                    else
                    {
                        _dbSet.Update(entity);
                    }
                }
                else
                {
                    _dbSet.Update(entity);
                }
            }

            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
        public virtual async Task<T?> GetByFirebaseIdAsync(string firebaseId)
        {
            // Verifica se a entidade tem a propriedade FirebaseId
            var property = typeof(T).GetProperty("FirebaseId", BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                throw new InvalidOperationException($"A entidade {typeof(T).Name} não possui a propriedade 'FirebaseId'.");

            // Usa expressão dinâmica para buscar
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var constant = Expression.Constant(firebaseId);
            var equal = Expression.Equal(propertyAccess, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equal, parameter);

            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(lambda);
        }
    }
}