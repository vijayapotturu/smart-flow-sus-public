using Assistant.Hub.Api.Core;
using Assistant.Hub.Api.Services.Profile;
using Assistants.API.Core;

using Microsoft.SemanticKernel;
using System.Text;

namespace MinimalApi.Services;

internal sealed class WorkflowManagerAgent
{
    private readonly ILogger<WorkflowManagerAgent> _logger;
    private readonly IConfiguration _configuration;
    private readonly OpenAIClientFacade _openAIClientFacade;
    private readonly AgentLog _agentLog;

    public WorkflowManagerAgent(OpenAIClientFacade openAIClientFacade,
                                ILogger<WorkflowManagerAgent> logger,
                                IConfiguration configuration,
                                AgentLog agentLog)
    {
        _openAIClientFacade = openAIClientFacade;
        _logger = logger;
        _configuration = configuration;
        _agentLog = agentLog;
    }

    public async Task<ResponseResult> ProcessesRequestAsync(TaskRequest request, ProfileDefinition profile, string kernelDeploymentName = ContextVariableOptions.KernelInstanceDynamicAgents, CancellationToken cancellationToken = default)
    {
        // Setup agent logging

        // Kernel and context setup
        var kernel = _openAIClientFacade.GetKernelByDeploymentName(kernelDeploymentName);
        var context = new KernelArguments
        {
            [ContextVariableOptions.TaskRequest] = request,
            [ContextVariableOptions.AgentLog] = _agentLog
        };

        foreach (var agent in profile.Agents)
        {
            var agentPlugin = kernel.Plugins.GetFunction(ContextVariableOptions.AgentTools, agent.Type);
            context[ContextVariableOptions.AgentDefinition] = agent;
            await kernel.InvokeAsync(agentPlugin, context, cancellationToken);
        }

        // Build Response
        // TODO: Piotr - handle different result formats

        var sb = new StringBuilder();
        foreach (var agent in profile.Agents)
        {
            if (context.ContainsName($"{agent.Name}Result"))
            {
                var agentResult = context[$"{agent.Name}Result"] as string;
                if (agentResult != null)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                    sb.AppendLine($"### {agent.Name}");
                    sb.AppendLine();
                    sb.AppendLine(agentResult);
                    sb.AppendLine();
                }
            }
        }

        //await _agentLog.PersistLogAsync();

        return new ResponseResult(sb.ToString(), _agentLog.LogEntries);
    }

}
