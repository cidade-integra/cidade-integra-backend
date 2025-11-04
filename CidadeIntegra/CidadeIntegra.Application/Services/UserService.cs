using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CidadeIntegra.Application.Interfaces.Repositories;
using CidadeIntegra.Application.Interfaces.Services;
using CidadeIntegra.Domain.Entities;

namespace CidadeIntegra.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("Iniciando busca de usuário pelo email: {Email}", email);
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogWarning("Nenhum usuário encontrado com o email: {Email}", email);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário por email: {Email}", email);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Buscando todos os usuários...");
                var users = await _userRepository.GetAllAsync();
                _logger.LogInformation("Busca concluída. Total de usuários encontrados: {Count}", users?.Count() ?? 0);

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os usuários");
                throw;
            }
        }

        public async Task CreateAsync(User user)
        {
            try
            {
                _logger.LogInformation("Criando novo usuário com ID Firebase: {FirebaseId}", user.FirebaseId);

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("Usuário criado com sucesso: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar usuário com ID Firebase: {FirebaseId}", user.FirebaseId);
                throw;
            }
        }

        public async Task<User?> GetByFirebaseIdAsync(string firebaseId)
        {
            try
            {
                _logger.LogInformation("Buscando usuário por FirebaseId: {FirebaseId}", firebaseId);
                var user = await _userRepository.GetByFirebaseIdAsync(firebaseId);

                if (user == null)
                {
                    _logger.LogWarning("Usuário não encontrado para FirebaseId: {FirebaseId}", firebaseId);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar usuário por FirebaseId: {FirebaseId}", firebaseId);
                throw;
            }
        }

        public async Task UpdateAsync(User user)
        {
            try
            {
                _logger.LogInformation("Atualizando usuário com ID: {UserId}", user.Id);
                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Usuário atualizado com sucesso: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar usuário com ID: {UserId}", user.Id);
                throw;
            }
        }
    }
}
