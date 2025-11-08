using CidadeIntegra.Application.Interfaces.Services;
using CidadeIntegra.Domain.Entities;
using CidadeIntegra.Infra.Data.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CidadeIntegra.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger<CommentService> _logger;

        public CommentService(ICommentRepository commentRepository, ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        public async Task<Comment?> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Buscando comentário por ID: {CommentId}", id);
                var comment = await _commentRepository.GetByIdAsync(id);

                if (comment == null)
                    _logger.LogWarning("Comentário não encontrado com ID: {CommentId}", id);

                return comment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar comentário com ID: {CommentId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Comment>> GetByReportIdAsync(Guid reportId)
        {
            try
            {
                _logger.LogInformation("Buscando comentários do report ID: {ReportId}", reportId);
                var comments = await _commentRepository.GetByReportIdAsync(reportId);
                _logger.LogInformation("Encontrados {Count} comentários para report ID: {ReportId}", comments.Count(), reportId);

                return comments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar comentários para report ID: {ReportId}", reportId);
                throw;
            }
        }

        public async Task<Comment?> GetByFirebaseIdAsync(string firebaseId)
        {
            try
            {
                _logger.LogInformation("Buscando comentário por FirebaseId: {FirebaseId}", firebaseId);
                var comment = await _commentRepository.GetByFirebaseIdAsync(firebaseId);

                if (comment == null)
                    _logger.LogWarning("Comentário não encontrado para FirebaseId: {FirebaseId}", firebaseId);

                return comment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar comentário por FirebaseId: {FirebaseId}", firebaseId);
                throw;
            }
        }

        public async Task CreateAsync(Comment comment)
        {
            try
            {
                _logger.LogInformation("Criando comentário para report ID: {ReportId}", comment.ReportId);
                await _commentRepository.AddAsync(comment);
                await _commentRepository.SaveChangesAsync();
                _logger.LogInformation("Comentário criado com sucesso: {CommentId}", comment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar comentário para report ID: {ReportId}", comment.ReportId);
                throw;
            }
        }

        public async Task UpdateAsync(Comment comment)
        {
            try
            {
                _logger.LogInformation("Atualizando comentário ID: {CommentId}", comment.Id);
                await _commentRepository.UpdateAsync(comment);
                await _commentRepository.SaveChangesAsync();
                _logger.LogInformation("Comentário atualizado com sucesso: {CommentId}", comment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar comentário ID: {CommentId}", comment.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Removendo comentário ID: {CommentId}", id);
                await _commentRepository.DeleteAsync(id);
                await _commentRepository.SaveChangesAsync();
                _logger.LogInformation("Comentário removido com sucesso: {CommentId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover comentário ID: {CommentId}", id);
                throw;
            }
        }
    }
}