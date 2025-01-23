using Assistants.API.Core;
using Azure.Core;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using Azure;
using System.Collections.Concurrent;

namespace Assistant.Hub.Api.Services.Search
{
    public class SearchClientFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, SearchClient> _clients = new ConcurrentDictionary<string, SearchClient>();
        private readonly TokenCredential? _credential;
        private readonly AzureKeyCredential? _keyCredential;

        public SearchClientFactory(IConfiguration configuration, TokenCredential? credential = null, AzureKeyCredential? keyCredential = null)
        {
            if(credential == null && keyCredential == null)
            {
                throw new ArgumentException("Either a token credential or an Azure key credential must be provided.");
            }
            _configuration = configuration;
            _credential = credential;
            _keyCredential = keyCredential;
        }

        public SearchClient GetOrCreateClient(string indexName)
        {
            // Check if a client for the given index already exists
            if (_clients.TryGetValue(indexName, out var client))
            {
                return client;
            }

            // Create a new client for the index
            var newClient = CreateClientForIndex(indexName);
            _clients[indexName] = newClient;
            return newClient;
        }

        private SearchClient CreateClientForIndex(string indexName)
        {
            if (_keyCredential != null)
            {
                return new SearchClient(new Uri(_configuration[AppConfigurationSetting.AzureAISearchEndpoint]), indexName, _keyCredential);
            }
            return new SearchClient(new Uri(_configuration[AppConfigurationSetting.AzureAISearchEndpoint]), indexName, _credential);
        }
    }
}
