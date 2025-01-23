using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Assistant.Hub.Api.Assistants;
using Assistant.Hub.Api.Core;
using Assistant.Hub.Api.Services.Profile;
using Assistant.Hub.Api.Services.Profile.Prompts;
using Assistants.API.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MinimalApi.Services.Skills;

public class AgentModel
{
    [KernelFunction("DynamicModelTask")]
    [Description("Execute a dynamic task with the LLM")]
    public async Task LLMTaskAsync(KernelArguments context, Kernel kernel)
    {
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        var request = context[ContextVariableOptions.TaskRequest] as TaskRequest;
        var agentDefinition = context[ContextVariableOptions.AgentDefinition] as AgentDefinition;
        var agentLog = context[ContextVariableOptions.AgentLog] as AgentLog;

        ArgumentNullException.ThrowIfNull(chatGpt, nameof(chatGpt));
        ArgumentNullException.ThrowIfNull(request, nameof(TaskRequest));
        ArgumentNullException.ThrowIfNull(agentDefinition, nameof(AgentDefinition));
        ArgumentNullException.ThrowIfNull(agentLog, nameof(AgentLog));

        if (agentDefinition.Tools != null)
        {
            foreach (var toolDefinition in agentDefinition.Tools)
            {
                var tool = kernel.Plugins.GetFunction(toolDefinition.Type, toolDefinition.Function);
                context[ContextVariableOptions.ToolDefinition] = toolDefinition;
                await kernel.InvokeAsync(tool, context);
            }
        }

        var sw = Stopwatch.StartNew();

        var promptSystemMessage = PromptService.ResolvePrompt(agentDefinition.SystemMessage, request);
        var promptUserTemplate = PromptService.ResolvePrompt(agentDefinition.UserMessage, request);

        var promptUserMessage = await PromptService.RenderPromptAsync(kernel, promptUserTemplate, context);
        var chatHistory = request.BuildChatHistory(promptSystemMessage, promptUserMessage);
        var response = await chatGpt.GetChatMessageContentAsync(chatHistory, DefaultSettings.AIChatRequestSettings, kernel);
        context[$"{agentDefinition.Name}Result"] = response.Content;

        sw.Stop();
        var sb = new StringBuilder();
        sb.AppendLine("## Prompt");
        sb.AppendLine(promptSystemMessage);
        sb.AppendLine(promptUserMessage);
        sb.AppendLine("## Response");
        sb.AppendLine(response.Content);
        agentLog.LogAgentStep(agentDefinition.Name, string.Empty, sb.ToString(), new AgentStepDiagnostics(sw.ElapsedMilliseconds));
    }

    [KernelFunction("ToolModelTask")]
    [Description("Execute a series of tools.")]
    public async Task ToolModelTaskAsync(KernelArguments context, Kernel kernel)
    {
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        var request = context[ContextVariableOptions.TaskRequest] as TaskRequest;
        var agentDefinition = context[ContextVariableOptions.AgentDefinition] as AgentDefinition;
        ArgumentNullException.ThrowIfNull(chatGpt, nameof(chatGpt));
        ArgumentNullException.ThrowIfNull(request, nameof(TaskRequest));
        ArgumentNullException.ThrowIfNull(agentDefinition, nameof(AgentDefinition));

        if (agentDefinition.Tools != null)
        {
            foreach (var toolDefinition in agentDefinition.Tools)
            {
                var tool = kernel.Plugins.GetFunction(toolDefinition.Type, toolDefinition.Function);
                context[ContextVariableOptions.ToolDefinition] = toolDefinition;
                await kernel.InvokeAsync(tool, context);
            }
        }
    }

    [KernelFunction("ReflectionAgent")]
    [Description("Execute a dynamic task with the LLM")]
    public async Task ReflectionAgentAsync(KernelArguments context, Kernel kernel)
    {
        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        var request = context[ContextVariableOptions.TaskRequest] as TaskRequest;
        var agentDefinition = context[ContextVariableOptions.AgentDefinition] as AgentDefinition;
        var reviewerSettings = agentDefinition.ReviewerSettings;
        ArgumentNullException.ThrowIfNull(chatGpt, nameof(chatGpt));
        ArgumentNullException.ThrowIfNull(request, nameof(TaskRequest));
        ArgumentNullException.ThrowIfNull(agentDefinition, nameof(AgentDefinition));
        ArgumentNullException.ThrowIfNull(reviewerSettings, nameof(ReviewSettings));

        if (agentDefinition.Tools != null)
        {
            foreach (var toolDefinition in agentDefinition.Tools)
            {
                var tool = kernel.Plugins.GetFunction(toolDefinition.Type, toolDefinition.Function);
                context[ContextVariableOptions.ToolDefinition] = toolDefinition;
                await kernel.InvokeAsync(tool, context);
            }
        }

        var analyzePromptSystemMessage = PromptService.ResolvePrompt(agentDefinition.SystemMessage, request);
        var analyzePromptUserTemplate = PromptService.ResolvePrompt(agentDefinition.UserMessage, request);
        var reviewPromptSystemMessage = PromptService.ResolvePrompt(reviewerSettings.SystemMessage, request);
        var reviewPromptUserTemplate = PromptService.ResolvePrompt(reviewerSettings.UserMessage, request);

        var complete = false;
        var attempts = 0;
        while (!complete)
        {
            var analyzeResult = Analyze(analyzePromptSystemMessage, analyzePromptUserTemplate, request, chatGpt, context, kernel);
            var result = await Review(reviewPromptSystemMessage, reviewPromptUserTemplate, request, chatGpt, context, kernel);
            attempts++;

            if (result.complete || reviewerSettings.MaxReviewAttempts >= attempts)
            {
                context[$"{agentDefinition.Name}Result"] = result;
                complete = true;
            }
        }
    }

    private async Task<string> Analyze(string systemMessage, string userMessage, TaskRequest request, IChatCompletionService chatGpt, KernelArguments context, Kernel kernel)
    {
        var promptUserMessage = await PromptService.RenderPromptAsync(kernel, userMessage, context);
        var chatHistory = request.BuildChatHistory(systemMessage, promptUserMessage);
        var result = await chatGpt.GetChatMessageContentAsync(chatHistory, DefaultSettings.AIChatRequestSettings, kernel);
        return result.Content;
    }

    private async Task<ReviewerResponse> Review(string systemMessage, string userMessage, TaskRequest request, IChatCompletionService chatGpt, KernelArguments context, Kernel kernel)
    {
        var promptUserMessage = await PromptService.RenderPromptAsync(kernel, userMessage, context);
        var chatHistory = request.BuildChatHistory(systemMessage, promptUserMessage);
        var result = await chatGpt.GetChatMessageContentAsync(chatHistory, DefaultSettings.JsonResponseSettings, kernel);
        var reviewerResponse = JsonSerializer.Deserialize<ReviewerResponse>(result.Content);
        return reviewerResponse;
    }
 
}
