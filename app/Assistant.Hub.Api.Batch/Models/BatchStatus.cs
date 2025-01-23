
﻿using Microsoft.AspNetCore.DataProtection.Repositories;

namespace Assistant.Hub.Api.Batch.Models;


public class BatchStatus
{
    public BatchStatus()
    {
        BatchItems = new List<LabelStatus>();
    }

    public static string StatusPending => "Pending";
    public static string StatusProcessing => "Processing";
    public static string StatusCompleted => "Completed";
    public static string StatusFailed => "Failed";

    public string Id { get; set; }

    public string? BatchId { get; set; }
    
    public string Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public IList<LabelStatus> BatchItems { get; set; }

    public void UpdateItemStatus(string blobName, string status)
    {
        var item = BatchItems.FirstOrDefault(_ => _.BlobName == blobName);
        if (item != null)
        {
            item.Status = status;
        }
    }

    public static BatchStatus FromBatchRequest(string instanceId, List<TaskRequest> batchItems)
    {
        var batchStatus = new BatchStatus
        {
            Id = instanceId,
            BatchId = instanceId,
            Status = StatusPending,
            CreatedAt = DateTime.UtcNow,
            BatchItems = batchItems.Select(x => new LabelStatus(x.Files.First().BlobName, StatusPending)).ToList()

        };

        return batchStatus;
    }
}
