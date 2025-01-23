// Copyright (c) Microsoft. All rights reserved.

using Assistant.Hub.Api.Core;
using Assistant.Hub.Api.Services.Profile;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Text;
using System.Text;

namespace Assistant.Hub.Api.Services.Search;

public class SearchLogic<T> where T : ISourceDataModel
{
    private readonly SearchClient _searchClient;
    private readonly AzureOpenAIClient _openAIClient;
    private readonly string _embeddingModelName;
    private readonly string _embeddingFieldName;
    private readonly List<string> _selectFields;

    public SearchLogic(AzureOpenAIClient openAIClient, SearchClientFactory factory, string indexName, string embeddingModelName, string embeddingFieldName, List<string> selectFields)
    {
        _searchClient = factory.GetOrCreateClient(indexName);
        _openAIClient = openAIClient;
        _embeddingModelName = embeddingModelName;
        _embeddingFieldName = embeddingFieldName;
        _selectFields = selectFields;
    }

    public async Task<VectorSearchResult> SearchAsync(string query, RAGSettingsSummary ragSettings, KernelArguments arguments)
    {
        // Generate the embedding for the query
        var queryEmbeddings = await GenerateEmbeddingsAsync(query, _openAIClient);

        var searchOptions = new SearchOptions
        {
            Size = ragSettings.DocumentRetrievalDocumentCount,
            VectorSearch = new()
            {
                Queries = { new VectorizedQuery(queryEmbeddings.ToArray()) { KNearestNeighborsCount = ragSettings.KNearestNeighborsCount, Fields = { _embeddingFieldName } } }
            }
        };

        foreach (var field in _selectFields)
        {
            searchOptions.Select.Add(field);
        }

        // Perform the search and build the results
        var response = await _searchClient.SearchAsync<T>(query, searchOptions);
        var list = new List<T>();
        foreach (var result in response.Value.GetResults())
        {
            list.Add(result.Document);
        }

        // Filter the results by the maximum request token size
        var sourceSummary = FilterByMaxRequestTokenSize(list, ragSettings.DocumentRetrievalMaxSourceTokens, ragSettings.CitationUseSourcePage);
        return sourceSummary;
    }

    private VectorSearchResult FilterByMaxRequestTokenSize(IReadOnlyList<T> sources, int maxRequestTokens, bool citationUseSourcePage)
    {
        int sourceSize = 0;
        int tokenSize = 0;
        var documents = new List<ISourceDataModel>();
        var tokenCounter = new TextChunker.TokenCounter(input => input.Split(' ').Length);
        var sb = new StringBuilder();
        foreach (var document in sources)
        {
            var text = document.FormatAsOpenAISourceText(citationUseSourcePage);
            sourceSize += text.Length;
            tokenSize += tokenCounter(text);
            if (tokenSize > maxRequestTokens)
            {
                break;
            }
            documents.Add(document);
            sb.AppendLine(text);
        }
        return new VectorSearchResult(sb.ToString(), documents);
    }

    private async Task<ReadOnlyMemory<float>> GenerateEmbeddingsAsync(string text, AzureOpenAIClient openAIClient)
    {
        var response = await openAIClient.GetEmbeddingClient(_embeddingModelName).GenerateEmbeddingsAsync(new List<string> { text });
        return response.Value[0].ToFloats();
    }
}
