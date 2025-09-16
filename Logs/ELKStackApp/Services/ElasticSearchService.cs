using Elastic.Clients.Elasticsearch;


namespace ELKStackApp.Services;

public class ElasticSearchService <T>(ElasticsearchClient elasticsearchClient) : IElasticSearchService<T>
    where T : class
{
    readonly string _index = typeof(T).Name.ToLowerInvariant(); // default index name based on type

    public async Task BulkIndexDocumentsAsync(IEnumerable<T> documents)
    {
        if(documents is null)
        {
            throw new ArgumentNullException(nameof(documents));
        }

        BulkResponse bulkResponse = await elasticsearchClient.BulkAsync(
            b => b
            .Index(_index)
                .IndexMany(documents));

        if(!bulkResponse.IsValidResponse || bulkResponse.Errors)
        {
            // You could inspect bulkResponse.ItemsWithErrors for details and throw/log as needed.
            throw new InvalidOperationException("Bulk index failed.");
        }
    }

    public async Task<long> CountDocumentsAsync()
    {
        CountResponse response = await elasticsearchClient.CountAsync(c => c.Index(_index));
        return response.IsValidResponse ? response.Count : 0;
    }

    public async Task<string> CreateDocumentAsync(T document)
    {
        // "Create" will fail if a doc with the same id already exists (upsert=false semantics)
        CreateResponse response = await elasticsearchClient.CreateAsync(document, c => c.Index(_index));
        return response.IsValidResponse ? "Ok" : "Fail";
    }

    public async Task<bool> DeleteDocumentByIdAsync(int id)
    {
        DeleteResponse response = await elasticsearchClient.DeleteAsync<T>(id, d => d.Index(_index));
        // If it didn't exist, Result may be NotFound; treat that as false.
        return response.IsValidResponse && (response.Result != Result.NotFound);
    }

    public async Task<bool> DocumentExistsAsync(int id)
    {
        ExistsResponse response = await elasticsearchClient.ExistsAsync<T>(id, e => e.Index(_index));
        return response.IsValidResponse && response.Exists;
    }

    public async Task<T> GetDocumentByIdAsync(int id)
    {
        GetResponse<T> response = await elasticsearchClient.GetAsync<T>(id, g => g.Index(_index));
        if(!response.IsValidResponse || !response.Found)
        {
            return null;
        }

        return response.Source;
    }

    public async Task<string> IndexDocumentAsync(T document)
    {
        // "Index" will create or overwrite as needed
        IndexResponse response = await elasticsearchClient.IndexAsync(document, i => i.Index(_index));
        return response.IsValidResponse ? "Ok" : "Fail";
    }

    public async Task<IEnumerable<T>> SearchDocumentsAsync(string query)
    {
        if((query is null) || (query == string.Empty))
        {
            query = "*";
        }

        SearchResponse<T> response = await elasticsearchClient.SearchAsync<T>(
            s => s
            .Index(_index)
                .Query(q => q.QueryString(qs => qs.Query(string.IsNullOrWhiteSpace(query) ? "*" : query)))
                .Size(1000) // cap to avoid huge payloads; tune as needed
        );

        if(!response.IsValidResponse)
        {
            return Enumerable.Empty<T>();
        }

        return response.Documents;
    }

    public async Task<IEnumerable<T>> SearchDocumentsAsync(Func<T, bool> predicate)
    {
        if(predicate is null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        // ES cannot translate a .NET Func<T,bool>—so we pull a limited set and filter in-memory.
        // For real usage, consider exposing a typed/filterable query instead of Func<T,bool>.
        SearchResponse<T> response = await elasticsearchClient.SearchAsync<T>(
            s => s
            .Index(_index)
                .Query(
                    q => q.MatchAll(
                            m =>
                            {
                            }))
                .Size(1000) // tune / paginate if needed
        );

        if(!response.IsValidResponse)
        {
            return Enumerable.Empty<T>();
        }

        return response.Documents.Where(predicate);
    }

    public async Task<bool> UpdateDocumentAsync(int id, T document)
    {
        UpdateResponse<T> response = await elasticsearchClient.UpdateAsync(
            new UpdateRequest<T, T>(_index, id)
            {
                Index = _index,
                Doc = document,
                DocAsUpsert = true,
            });
        return response.IsValidResponse;
    }
}