using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Infra.Data.Context;
using CidadeIntegra.Infra.Data.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CidadeIntegra.Infra.Data.Repositories
{
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetByReportIdAsync(Guid reportId)
        {
            return await _context.Comments
                .Where(c => c.ReportId == reportId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}