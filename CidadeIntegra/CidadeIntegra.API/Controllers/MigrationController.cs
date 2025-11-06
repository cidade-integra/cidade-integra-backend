using CidadeIntegra.API.Attributes;
using CidadeIntegra.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CidadeIntegra.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuthorize]
    public class MigrationController : ControllerBase
    {
        private readonly IMigrationService _migrationService;
        private readonly ILogger<MigrationController> _logger;

        public MigrationController(IMigrationService migrationService, ILogger<MigrationController> logger)
        {
            _migrationService = migrationService;
            _logger = logger;
        }

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