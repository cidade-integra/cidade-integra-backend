using CidadeIntegra.Infra.Data.Interfaces.Provider;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;

namespace CidadeIntegra.Infra.Data.Firebase
{
    public class FirestoreProvider : IFirestoreProvider
    {
        private readonly FirestoreDb _firestore;

        public FirestoreProvider(IConfiguration configuration)
        {
            var projectId = configuration["Firebase:ProjectId"];
            var serviceAccountPath = configuration["Firebase:ServiceAccountPath"];

            _firestore = FirebaseInitializer.InitializeFirestore(projectId, serviceAccountPath);
        }

        public async Task<IReadOnlyList<Dictionary<string, object>>> GetDocumentsAsync(string collectionName)
        {
            var snapshot = await _firestore.Collection(collectionName).GetSnapshotAsync();
            return snapshot.Documents.Select(d => d.ToDictionary()).ToList();
        }
    }
}