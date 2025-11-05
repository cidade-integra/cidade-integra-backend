using CidadeIntegra.Infra.Data.Firebase;
using Google.Cloud.Firestore;
using Microsoft.OpenApi.Models;
using CidadeIntegra.Infra.IoC;
using CidadeIntegra.API.Middlewares;

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
            // Configura o Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Cidade Integra API",
                    Version = "v1",
                    Description = "API para gestão e visualização de denúncias urbanas."
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

            // Add services to the container.
            builder.Services.AddInfrastructureAPI(builder.Configuration);

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