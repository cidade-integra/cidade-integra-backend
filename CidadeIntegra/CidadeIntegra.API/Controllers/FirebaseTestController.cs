using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

namespace CidadeIntegra.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FirebaseTestController : ControllerBase
    {
        private readonly FirestoreDb _firestore;

        public FirebaseTestController(FirestoreDb firestore)
        {
            _firestore = firestore;
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestConnection()
        {
            // Cria documento de teste
            var testDoc = _firestore.Collection("testes").Document("teste-1");

            await testDoc.SetAsync(new
            {
                message = "Conexão bem-sucedida!",
                timestamp = DateTime.UtcNow
            });

            // Lê o documento para validar
            var snapshot = await testDoc.GetSnapshotAsync();
            var data = snapshot.ToDictionary();

            return Ok(new
            {
                sucesso = true,
                mensagem = "Conexão com Firestore estabelecida!",
                dados = data
            });
        }
    }
}
