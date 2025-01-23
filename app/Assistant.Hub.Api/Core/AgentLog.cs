using System.Text;
using Microsoft.Azure.Cosmos;

namespace Assistant.Hub.Api.Core;

public class AgentLog
{
    private readonly CosmosClient _cosmosClient;
    private readonly StringBuilder _log = new StringBuilder();
    private readonly Guid _requestId;
    private readonly List<AgentLogEntry> _LogEntries;

    public AgentLog(Guid requestId, CosmosClient cosmosClient)
    {
        ArgumentNullException.ThrowIfNull(cosmosClient, nameof(cosmosClient));
        _requestId = requestId;
        _cosmosClient = cosmosClient;

        _LogEntries = new List<AgentLogEntry>();
    }

    public void LogAgentStep(string agentName, string step, string? content = null, AgentStepDiagnostics? diagnostics = null)
    {
        _log.AppendLine($"### {agentName}");
        _log.AppendLine(step);

        if(!string.IsNullOrWhiteSpace(content))
        {
            _log.AppendLine(content);
        }

        _LogEntries.Add(new AgentLogEntry(agentName, step, content, diagnostics));
    }

    public string Log => _log.ToString();
    public List<AgentLogEntry> LogEntries => _LogEntries;

    public async Task PersistLogAsync()
    {
        var container = _cosmosClient.GetContainer("AgentLog", "AgentLog");
        var agentLog = new
        {
            requestId = _requestId.ToString(),
            log = Log
        };

        await container.UpsertItemAsync(agentLog);
    }
}

public record AgentLogEntry(string agentName, string step, string? content, AgentStepDiagnostics? diagnostics);

public record AgentStepDiagnostics(long ElapsedMilliseconds);
