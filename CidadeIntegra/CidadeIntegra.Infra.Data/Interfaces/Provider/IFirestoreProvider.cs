namespace CidadeIntegra.Infra.Data.Interfaces.Provider
{
    public interface IFirestoreProvider
    {
        Task<IReadOnlyList<Dictionary<string, object>>> GetDocumentsAsync(string collectionName);
    }
}