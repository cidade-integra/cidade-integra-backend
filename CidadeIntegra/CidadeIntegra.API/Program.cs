using CidadeIntegra.Infra.Data.Firebase;
using Google.Cloud.Firestore;

namespace CidadeIntegra.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
                // Configurações do Firebase (puxe do appsettings.json)
            var projectId = builder.Configuration["Firebase:ProjectId"];
            var serviceAccountPath = builder.Configuration["Firebase:ServiceAccountPath"];

            // Inicializa conexão com Firestore
            FirestoreDb firestore = FirebaseInitializer.InitializeFirestore(projectId, serviceAccountPath);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Injeta Firestore no container de dependência
            builder.Services.AddSingleton(firestore);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
