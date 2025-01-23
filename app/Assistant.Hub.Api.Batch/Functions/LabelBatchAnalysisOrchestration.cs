using Assistant.Hub.Api.Batch.Functions.Activities;
using Assistant.Hub.Api.Batch.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Assistant.Hub.Api.Batch.Functions;

public class LabelBatchAnalysisOrchestration
{
    [Function(nameof(LabelBatchAnalysisOrchestration))]
    public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger<LabelBatchAnalysisOrchestration>();
        var batch = context.GetInput<List<TaskRequest>>();

        ArgumentNullException.ThrowIfNull(batch, nameof(batch));

        // Map the input batch to a status object
        var status = BatchStatus.FromBatchRequest(context.InstanceId, batch);

        try
        {
            // Check to see if we have labels to process in the input
            if (batch.Count == 0)
            {
                logger.LogWarning("No labels to process");
                return;
            }

            // Save the initial status to CosmosDB
            await context.CallActivityAsync(nameof(PersistBatchStatusActivity), status);    

            var outputs = new List<LabelAnalysisResult>();
            var images = new List<byte[]>();

            // Iterate over batch of labels to process
            foreach (var label in batch)
            {
                logger.LogInformation($"Processing blob {label.Files.First().BlobName}");

                // Update status for current label to Processing and save updated status to Cosmos
                status.UpdateItemStatus(label.Files.First().BlobName, BatchStatus.StatusProcessing);
                await context.CallActivityAsync(nameof(PersistBatchStatusActivity), status);

                // Fetch the image to be analyzed from blob storage
                // TODO: Future enhancement, clean up blobs after processing?  or maybe just rely on an aging policy to auto-delete them
                var labelImage = await context.CallActivityAsync<byte[]>(nameof(GetBlobForAnalysisActivity), label.Files.First().BlobName);
                images.Add(labelImage);

                // Call Assistant API to analyze the label
                var result = await context.CallActivityAsync<LabelAnalysisResult>(nameof(AnalyzeLabelActivity), label);
                var resultToSave = new AnalysisResultToSave(status.BatchId, $"{Path.GetFileNameWithoutExtension(label.Files.First().BlobName)}_{DateTime.UtcNow.ToString("yyyyMMddHHmss")}.md", result.AnalysisResult);

                // Save the analysis result to blob storage
                await context.CallActivityAsync(nameof(SaveLabelAnalysisResultActivity), resultToSave);

                // Update status for current label to Completed and save updated status to Cosmos
                status.UpdateItemStatus(label.Files.First().BlobName, BatchStatus.StatusCompleted);
                await context.CallActivityAsync(nameof(PersistBatchStatusActivity), status);
            }

            // Update the batch status to Completed and save to Cosmos
            status.Status = BatchStatus.StatusCompleted;
            await context.CallActivityAsync(nameof(PersistBatchStatusActivity), status);
        }
        catch (Exception e)
        {
            logger.LogError(e,"error processing label batch");

            status.Status = BatchStatus.StatusFailed;
            await context.CallActivityAsync(nameof(PersistBatchStatusActivity), status);

            throw;
        }
    }
}