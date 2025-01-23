namespace Assistant.Hub.Api.Batch.Models;

public class LabelStatus
{
    public LabelStatus(string blobName, string status)
    {
        BlobName = blobName;
        Status = status;
    }

    public string BlobName { get; set; }

    public string Status { get; set; }
}