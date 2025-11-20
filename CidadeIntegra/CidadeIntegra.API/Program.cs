using CidadeIntegra.API.Middlewares;
using CidadeIntegra.Infra.Data.Firebase;
using CidadeIntegra.Infra.IoC;
using DotNetEnv;
using Google.Cloud.Firestore;
using Microsoft.OpenApi.Models;

namespace CidadeIntegra.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configura��o Vari�veis de Ambiente
            builder.Configuration.AddEnvironmentVariables();
            Env.Load();

#if DEBUG
            var testKey = Environment.GetEnvironmentVariable("MIGRATION_API_KEY");
            if (string.IsNullOrEmpty(testKey))
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine($"MIGRATION_API_KEY: {(string.IsNullOrEmpty(testKey) ? "n�o encontrada" : "carregada com sucesso")}");
            Console.ResetColor();
#endif

            #endregion

            #region Configurações Firebase

            var projectId = builder.Configuration["Firebase:ProjectId"];

            // caminho dinâmico que funciona no Docker e Windows
            var serviceAccountPath = Path.Combine(AppContext.BaseDirectory, "firebase-key.json");

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", serviceAccountPath);
            

            FirestoreDb firestore = FirebaseInitializer.InitializeFirestore(projectId, serviceAccountPath);

            builder.Services.AddSingleton(firestore);

            #endregion

            #region Configura��es Swagger
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

            #region Configura��o CORS
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