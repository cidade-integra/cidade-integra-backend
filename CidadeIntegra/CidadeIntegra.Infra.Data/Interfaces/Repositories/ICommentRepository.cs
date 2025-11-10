using CidadeIntegra.Application.Interfaces.Repositories;
using CidadeIntegra.Domain.Entities;

namespace CidadeIntegra.Infra.Data.Interfaces.Repositories
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetByReportIdAsync(Guid reportId);
    }
}