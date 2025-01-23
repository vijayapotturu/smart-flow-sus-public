namespace Assistant.Hub.Api.Batch.Models;

public record BatchLabelAnalysisItem(string ProductName, string RegulatorNumber, string Country, string BlobPath);

public record BatchAccepted(string InstanceId);

public record LabelAnalysisResult(string ProductName, string RegulatorNumber, string Country, string AnalysisResult);

public record AnalysisResultToSave(string BatchId, string FileName, string Result);

public record RequestFile(string Name, string? DataUrl, string? BlobName, Dictionary<string, string>? Metadata = null);

public record TaskRequest(Guid TaskId, string RequestMessage, IEnumerable<RequestFile> Files, Dictionary<string, string?> Prompts);
