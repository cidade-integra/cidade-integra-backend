using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace CidadeIntegra.Infra.Data.Firebase
{
    public static class FirebaseInitializer
    {
        public static FirestoreDb InitializeFirestore(string projectId, string serviceAccountPath)
        {
            using var stream = new FileStream(serviceAccountPath, FileMode.Open, FileAccess.Read);
            var credential = GoogleCredential.FromStream(stream);

            FirebaseApp.Create(new AppOptions
            {
                Credential = credential,
                ProjectId = projectId
            });

            var firestore = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                Credential = credential
            }.Build();

            return firestore;
        }
    }
}