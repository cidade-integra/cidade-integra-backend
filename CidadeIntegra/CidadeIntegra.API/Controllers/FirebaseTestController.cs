using CidadeIntegra.API.Attributes;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

namespace CidadeIntegra.API.Controllers
{
    /// <summary>
    /// Controlador responsável por testar a conexão com o banco de dados Firestore (NoSQL).
    /// </summary>
    /// <remarks>
    /// Esse controlador permite validar se a comunicação com o Firestore está funcionando corretamente,
    /// criando e lendo um documento de teste na coleção <c>testes</c>.
    /// 
    /// <para>Requer autenticação via chave de API (<see cref="ApiKeyAuthorizeAttribute"/>).</para>
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    [ApiKeyAuthorize]
    public class FirebaseTestController : ControllerBase
    {
        private readonly FirestoreDb _firestore;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="FirebaseTestController"/>.
        /// </summary>
        /// <param name="firestore">Instância do banco de dados <see cref="FirestoreDb"/> usada para executar operações de teste.</param>
        public FirebaseTestController(FirestoreDb firestore)
        {
            _firestore = firestore;
        }

        /// <summary>
        /// Testa a conexão com o Firestore criando e lendo um documento de teste.
        /// </summary>
        /// <remarks>
        /// Esse endpoint cria um documento chamado <c>teste-1</c> na coleção <c>testes</c> contendo
        /// uma mensagem e um carimbo de data/hora. Em seguida, o documento é lido novamente
        /// para confirmar a conexão com o Firestore.
        /// 
        /// /// <para><b>Autorização:</b></para>
        /// É necessário enviar uma chave de API válida no cabeçalho da requisição:
        /// <code>x-api-key: SUA_CHAVE_DE_API</code>
        ///
        /// <para><b>Exemplo de requisição:</b></para>
        /// <code>
        /// GET /api/firebasetest/test
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
        ///     <description>Retorna mensagem indicando sucesso na conexão e dados do documento criado.</description>
        ///   </item>
        ///   <item>
        ///     <term>500 Internal Server Error</term>
        ///     <description>Retorna mensagem de erro caso ocorra falha ao conectar ou manipular o Firestore.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <response code="200">Conexão com o Firestore estabelecida com sucesso.</response>
        /// <response code="500">Erro durante a conexão com o Firestore.</response>
        /// <returns>Um objeto JSON contendo o resultado da conexão e os dados do documento de teste.</returns>
        [HttpGet("test")]
        [ApiKeyAuthorize]
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