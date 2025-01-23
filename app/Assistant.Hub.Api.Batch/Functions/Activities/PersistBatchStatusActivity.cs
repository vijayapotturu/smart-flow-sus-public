using Assistant.Hub.Api.Batch.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Assistant.Hub.Api.Batch.Functions.Activities;

public class PersistBatchStatusActivity
{
    private readonly CosmosClient _cosmosClient;
    private readonly string _databaseName;
    private readonly string _containerName;

    public PersistBatchStatusActivity(CosmosClient cosmosClient,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient, nameof(cosmosClient));
        _cosmosClient = cosmosClient;

        var databaseName = configuration[ConfigurationKeys.CosmosDbDatabaseName];
        var containerName = configuration[ConfigurationKeys.CosmosDbContainerName];

        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);

        _containerName = containerName;
        _databaseName = databaseName;
    }

    [Function(nameof(PersistBatchStatusActivity))]
    public async Task RunAsync([ActivityTrigger] BatchStatus batchStatus, FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(batchStatus, nameof(batchStatus));
        var logger = context.GetLogger(nameof(PersistBatchStatusActivity));

        try
        {
            logger.LogInformation($"Saving batch {batchStatus.BatchId} to CosmosDB");

            var container = _cosmosClient.GetContainer(_databaseName, _containerName);
            await container.UpsertItemAsync(batchStatus);

            logger.LogInformation($"Successfully saved batch {batchStatus.BatchId} to CosmosDB");
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception caught while persisting status to CosmosDB: {e.Message}");
            throw;
        }
    }
}
