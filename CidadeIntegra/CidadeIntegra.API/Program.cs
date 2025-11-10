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

            #region Configurações Firebase
            // Configurações do Firebase (appsettings.json)
            var projectId = builder.Configuration["Firebase:ProjectId"];
            var serviceAccountPath = builder.Configuration["Firebase:ServiceAccountPath"];

            // Inicializa conexão com Firestore
            FirestoreDb firestore = FirebaseInitializer.InitializeFirestore(projectId, serviceAccountPath);

            // Injeta Firestore no container de dependência
            builder.Services.AddSingleton(firestore);
            #endregion

            #region Configurações Swagger
            // configura o Swagger para documentação e testes da API
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                // define informações básicas da API
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Cidade Integra API",
                    Version = "v1",
                    Description = "API para migração de dados do Firestore para SQL Server"
                });

                // adiciona suporte a autenticação via API Key
                c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                {
                    Description = "Chave de autenticação necessária para acessar endpoints protegidos.\n" +
                                  "Insira no header: 'x-api-key'.",
                    Name = "x-api-key",              // nome do header
                    In = ParameterLocation.Header,    // local de envio do header
                    Type = SecuritySchemeType.ApiKey, // tipo de autenticação
                    Scheme = "ApiKeyScheme"
                });

                // aplica a exigência da API key globalmente em todos os endpoints
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

            #region Configuração CORS
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

            #region Configuração Logging Global
            builder.Logging.ClearProviders(); // Remove qualquer configuração padrão de log
            builder.Logging.AddConsole(); // Envia todos os logs para o console
            builder.Logging.AddDebug(); // Envia logs para o Visual Studio Debug Output
            builder.Logging.SetMinimumLevel(LogLevel.Information); // Define o nível mínimo de log a ser registrado
            #endregion

            #region Configuração Variáveis de Ambiente
            builder.Configuration.AddEnvironmentVariables();
            Env.Load();

            #if DEBUG
                var testKey = Environment.GetEnvironmentVariable("MIGRATION_API_KEY");
                if (string.IsNullOrEmpty(testKey))
                    Console.ForegroundColor = ConsoleColor.Red;
                else
                    Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine($"MIGRATION_API_KEY: {(string.IsNullOrEmpty(testKey) ? "não encontrada" : "carregada com sucesso")}");
                Console.ResetColor();
            #endif

            #endregion

            #region Configuração IoC
            // Add services to the container.
            builder.Services.AddInfrastructureAPI(builder.Configuration);
            #endregion

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            #region Configuração Pipeline Swagger
            // Configuração do pipeline de requisição
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    // Título e endpoint da documentação
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cidade Integra API v1");
                });
            }
            #endregion

            #region Configuração Middleware
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            #endregion

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}