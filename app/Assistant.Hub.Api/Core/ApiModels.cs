
using Assistant.Hub.Api.Core;

namespace Assistants.API.Core;

public record ResponseResult(dynamic Answer, List<AgentLogEntry> ThoughtProcess, string? Error = null);

//public record ChatTurn(string User, IEnumerable<RequestFile> Files, string? Assistant = null);


public record RequestFile(string Name, string? DataUrl, string? BlobName, Dictionary<string, string>? Metadata = null);


public record TaskRequest(Guid TaskId, string RequestMessage, IEnumerable<RequestFile> Files, Dictionary<string, string>? prompts);

public record IndexRequest(string indexName);

public record IndexDocumentRequest(string indexName, string key, string sourceName, string? dataUrl, string? blobName);

public record ProductMasterInformation(IEnumerable<ProductMasterItem> Items);

public record ProductMasterItem(string Name, string Value);
