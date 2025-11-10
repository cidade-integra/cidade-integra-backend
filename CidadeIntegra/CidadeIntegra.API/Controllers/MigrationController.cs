using CidadeIntegra.API.Attributes;
using CidadeIntegra.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CidadeIntegra.API.Controllers
{
    /// <summary>
    /// Controlador responsável por executar a migração dos dados entre o banco NoSQL (Firebase)
    /// e o banco relacional SQL Server.
    /// </summary>
    /// <remarks>
    /// Esse endpoint dispara o processo de migração, utilizando o serviço <see cref="IMigrationService"/>.
    /// É protegido por autenticação via chave de API (<see cref="ApiKeyAuthorizeAttribute"/>).
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuthorize]
    public class MigrationController : ControllerBase
    {
        private readonly IMigrationService _migrationService;
        private readonly ILogger<MigrationController> _logger;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="MigrationController"/>.
        /// </summary>
        /// <param name="migrationService">Serviço responsável por executar a migração dos dados.</param>
        /// <param name="logger">Instância de logger utilizada para registrar logs de execução e erros.</param>
        public MigrationController(IMigrationService migrationService, ILogger<MigrationController> logger)
        {
            _migrationService = migrationService;
            _logger = logger;
        }

        /// <summary>
        /// Executa o processo de migração entre o banco NoSQL (Firebase) e o banco SQL Server.
        /// </summary>
        /// <remarks>
        /// Esse endpoint é utilizado para disparar manualmente o processo de backup e transferência dos dados.
        /// 
        /// <para><b>Autorização:</b></para>
        /// É necessário enviar uma chave de API válida no cabeçalho da requisição:
        /// <code>x-api-key: SUA_CHAVE_DE_API</code>
        ///
        /// <para><b>Exemplo de requisição:</b></para>
        /// <code>
        /// POST /api/migration/run
        /// Host: localhost:5001
        /// x-api-key: SUA_CHAVE_DE_API
        /// </code>
        ///
        /// <para><b>Respostas possíveis:</b></para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Código</term>
        ///     <description>Descrição</description>
        ///   </listheader>
        ///   <item>
        ///     <term>200 OK</term>
        ///     <description>Retorna mensagem de sucesso indicando que a migração foi concluída.</description>
        ///   </item>
        ///   <item>
        ///     <term>500 Internal Server Error</term>
        ///     <description>Retorna uma mensagem de erro caso ocorra alguma falha durante o processo.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <response code="200">Migração concluída com sucesso.</response>
        /// <response code="500">Erro durante a execução da migração.</response>
        /// <returns>Mensagem indicando o status da execução.</returns>
        [HttpPost("run")]
        [ApiKeyAuthorize]
        public async Task<IActionResult> RunMigration()
        {
            try
            {
                await _migrationService.RunMigrationAsync();
                return Ok(new { message = "Migração concluída com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante a migração.");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}