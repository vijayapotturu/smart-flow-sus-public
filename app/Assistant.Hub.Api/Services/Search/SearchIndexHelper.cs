using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;

public static class SearchIndexHelper
{
    public static async Task EnsureIndexExists(this SearchIndexClient indexClient, string indexName, CancellationToken cancellationToken = default)
    {
        // Try to retrieve the index. If it doesn't exist, a 404 will be thrown.
        try
        {
            await indexClient.GetIndexAsync(indexName, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            var definition = new SearchIndex(indexName)
            {
                Fields =
                {
                    new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                    new SimpleField("source", SearchFieldDataType.String) { IsFilterable = true },
                    new SimpleField("content", SearchFieldDataType.String) { IsSortable = false }
                }
            };
            await indexClient.CreateIndexAsync(definition,cancellationToken);
        }
    }

    public static async Task AddDocumentAsync(this SearchClient searchClient, string key, string content, string source)
    {
        var document = new
        {
            id = key,
            content = content,
            //source = source
        };
        var batch = IndexDocumentsBatch.Upload(new[] { document });
        IndexDocumentsResult result = await searchClient.IndexDocumentsAsync(batch);
    }    
}
