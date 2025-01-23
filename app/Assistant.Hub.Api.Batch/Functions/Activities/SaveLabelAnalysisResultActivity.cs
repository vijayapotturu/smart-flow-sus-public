using System.Runtime.CompilerServices;
using Assistant.Hub.Api.Batch.Models;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Assistant.Hub.Api.Batch.Functions.Activities;

public class SaveLabelAnalysisResultActivity
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public SaveLabelAnalysisResultActivity(BlobServiceClient blobServiceClient,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(blobServiceClient, nameof(blobServiceClient));
        _blobServiceClient = blobServiceClient;

        var containerName = configuration[ConfigurationKeys.BatchAnalysisStorageOutputContainerName];
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName, "Storage container is not configured");
        _containerName = containerName;
    }

    [Function(nameof(SaveLabelAnalysisResultActivity))]
    public async Task RunAsync([ActivityTrigger] AnalysisResultToSave result, FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));

        var logger = context.GetLogger(nameof(SaveLabelAnalysisResultActivity));

        try
        {
            logger.LogInformation($"Uploading blob {result.BatchId}/{result.FileName}");

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var uploadData = new BinaryData(result.Result);
            await containerClient.UploadBlobAsync($"{result.BatchId}/{result.FileName}", uploadData);

            logger.LogInformation($"Successfully uploaded results to {result.BatchId}/{result.FileName}");
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Exception caught uploading analysis result document: {e.Message}");
            throw;
        }
    }
}
