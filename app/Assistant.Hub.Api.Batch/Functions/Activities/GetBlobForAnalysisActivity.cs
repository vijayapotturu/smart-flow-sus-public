using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Assistant.Hub.Api.Batch.Functions.Activities;

public class GetBlobForAnalysisActivity
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public GetBlobForAnalysisActivity(BlobServiceClient blobServiceClient,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(blobServiceClient, nameof(blobServiceClient));
        _blobServiceClient = blobServiceClient;

        var containerName = configuration[ConfigurationKeys.BatchAnalysisStorageInputContainerName];
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName, nameof(containerName));
        _containerName = containerName;
    }

    [Function(nameof(GetBlobForAnalysisActivity))]
    public async Task<byte[]> RunAsync([ActivityTrigger] string blobPath, FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(blobPath, nameof(blobPath));

        var logger = context.GetLogger(nameof(GetBlobForAnalysisActivity));
        byte[] result;

        try
        {
            logger.LogInformation($"Downloading blob {blobPath}");
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobContent = await containerClient.GetBlobClient(blobPath).DownloadContentAsync();
            result = blobContent.Value.Content.ToArray();
            logger.LogInformation($"Downloaded blob {blobPath} successfully");
        }
        catch (Exception e)
        {
            logger.LogError(e, $"exception caught downloading blob {blobPath}: {e.Message}");
            throw;
        }

        return result;
    }
}
