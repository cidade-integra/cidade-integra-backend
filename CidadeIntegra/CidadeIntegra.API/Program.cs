using CidadeIntegra.API.Middlewares;
using CidadeIntegra.API.Options;
using CidadeIntegra.Infra.Data.Firebase;
using CidadeIntegra.Infra.IoC;
using Google.Cloud.Firestore;
using Microsoft.OpenApi.Models;

namespace CidadeIntegra.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configurações Firebase

            // Lê valores do ambiente (GitHub Actions, Docker ou local)
            var firebaseProjectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
            var firebaseKeyJson = Environment.GetEnvironmentVariable("FIREBASE_KEY_JSON");

            string tempServiceAccountPath = null!;

            try
            {
                // Cria arquivo temporário com as credenciais se a variável existir
                if (!string.IsNullOrEmpty(firebaseKeyJson))
                {
                    tempServiceAccountPath = Path.Combine(Path.GetTempPath(), "firebase-key.json");
                    File.WriteAllText(tempServiceAccountPath, firebaseKeyJson);

                    if (!File.Exists(tempServiceAccountPath))
                        throw new FileNotFoundException("O arquivo de credenciais Firebase não pôde ser criado.");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Firebase credentials created at {tempServiceAccountPath}");
                    Console.ResetColor();
                }
                else
                {
                    throw new InvalidOperationException("Variável de ambiente FIREBASE_KEY_JSON não encontrada.");
                }

                if (string.IsNullOrEmpty(firebaseProjectId))
                    throw new InvalidOperationException("Variável de ambiente FIREBASE_PROJECT_ID não encontrada.");

                FirestoreDb firestore = FirebaseInitializer.InitializeFirestore(firebaseProjectId, tempServiceAccountPath);

                // injeta Firestore no container de dependência
                builder.Services.AddSingleton(firestore);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erro ao inicializar Firebase: {ex.Message}");
                Console.ResetColor();

                throw;
            }

            #endregion

            #region Configuracoes Swagger
            // configura o Swagger para documenta��o e testes da API
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                // define informa��es b�sicas da API
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Cidade Integra API",
                    Version = "v1",
                    Description = "API para migra��o de dados do Firestore para SQL Server"
                });

                // adiciona suporte a autentica��o via API Key
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "Chave de autentica��o necess�ria para acessar endpoints protegidos.\n" +
                                  "Insira no header: 'x-api-key'.",
                    Name = "x-api-key",              // nome do header
                    In = ParameterLocation.Header,    // local de envio do header
                    Type = SecuritySchemeType.ApiKey, // tipo de autentica��o
                    Scheme = "ApiKeyScheme"
                });

                // aplica a exig�ncia da API key globalmente em todos os endpoints
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ApiKey"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            #endregion

            #region Configuracao CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("OpenCors", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            #endregion

            #region Configura��o Logging Global
            builder.Logging.ClearProviders(); // Remove qualquer configura��o padr�o de log
            builder.Logging.AddConsole(); // Envia todos os logs para o console
            builder.Logging.AddDebug(); // Envia logs para o Visual Studio Debug Output
            builder.Logging.SetMinimumLevel(LogLevel.Information); // Define o n�vel m�nimo de log a ser registrado
            #endregion

            #region Configuração MIGRATION_API_KEY

            builder.Services.Configure<MigrationOptions>(options =>
            {
                options.ApiKey = Environment.GetEnvironmentVariable("MIGRATION_API_KEY") ?? string.Empty;
            });

            var apiKey = Environment.GetEnvironmentVariable("MIGRATION_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("MIGRATION_API_KEY not found in environment variables!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("MIGRATION_API_KEY loaded successfully.");
            }
            Console.ResetColor();

            #endregion

            #region Configura��o IoC
            // Add services to the container.
            builder.Services.AddInfrastructureAPI(builder.Configuration);
            #endregion

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            #region Configura��o Pipeline Swagger
            // Configura��o do pipeline de requisi��o
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    // T�tulo e endpoint da documenta��o
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cidade Integra API v1");
                });
            }
            #endregion

            #region Configura��o Middleware
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            #endregion

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}