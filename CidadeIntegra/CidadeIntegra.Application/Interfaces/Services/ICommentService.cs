using CidadeIntegra.Domain.Entities;

namespace CidadeIntegra.Application.Interfaces.Services
{
    public interface ICommentService
    {
        Task<Comment?> GetByIdAsync(Guid id);
        Task<IEnumerable<Comment>> GetByReportIdAsync(Guid reportId);
        Task<Comment?> GetByFirebaseIdAsync(string firebaseId);
        Task CreateAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(Guid id);
    }
}