
namespace ELKStackApp.Services;

public interface IElasticSearchService <T>
{
    Task BulkIndexDocumentsAsync(IEnumerable<T> documents);

    Task<long> CountDocumentsAsync();

    Task<string> CreateDocumentAsync(T document);

    Task<bool> DeleteDocumentByIdAsync(int id);

    Task<bool> DocumentExistsAsync(int id);

    Task<T> GetDocumentByIdAsync(int id);

    Task<string> IndexDocumentAsync(T document);

    Task<IEnumerable<T>> SearchDocumentsAsync(string query);

    Task<IEnumerable<T>> SearchDocumentsAsync(Func<T, bool> predicate);

    Task<bool> UpdateDocumentAsync(int id, T document);
}