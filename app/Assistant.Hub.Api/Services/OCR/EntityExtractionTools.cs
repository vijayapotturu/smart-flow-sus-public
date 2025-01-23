// Copyright (c) Microsoft. All rights reserved.

using Assistant.Hub.Api.Assistants;
using Assistant.Hub.Api.Core;
using Assistant.Hub.Api.Services.Profile;
using Assistant.Hub.Api.Services.Profile.Prompts;
using Assistants.API.Core;
using Assistants.Hub.API.Core;
using Azure.Storage.Blobs;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace Assistant.Hub.Api.Services.Search;

public sealed class EntityExtractionTools
{
    private readonly BlobContainerClient _blobContainerClient;
    public EntityExtractionTools(BlobContainerClient blobContainerClient)
    {
        _blobContainerClient = blobContainerClient;
    }

    [KernelFunction("LLMTextExtraction"), Description("Extract Named Values From Image")]
    public async Task LLMTextExtractionAsync(KernelArguments arguments, Kernel kernel)
    {
        var toolDefinition = arguments[ContextVariableOptions.ToolDefinition] as ToolDefinition;
        var taskRequest = arguments[ContextVariableOptions.TaskRequest] as TaskRequest;

        ArgumentNullException.ThrowIfNull(toolDefinition, "toolDefinition not set.");
        ArgumentNullException.ThrowIfNull(taskRequest, "taskRequest not set.");
        ArgumentNullException.ThrowIfNull(toolDefinition.RAGSettings?.RequestFileName, "toolDefinition.RAGSettings.RequestFileName not set.");

        var file = taskRequest.Files.FirstOrDefault(x => x.Name == toolDefinition.RAGSettings.RequestFileName);

        ArgumentNullException.ThrowIfNull(file, $"file '{toolDefinition.RAGSettings.RequestFileName}' not found in the request");

        var dataUrl = file.GetDataUrlForPngJpeg();
        
        if (dataUrl == null)
        {
            var blobClient = _blobContainerClient.GetBlobClient(file.BlobName);
            var blobDownloadInfo = await blobClient.DownloadContentAsync();
            string base64Content = Convert.ToBase64String(blobDownloadInfo.Value.Content);
            string mimeType = "image/png";
            dataUrl = $"data:{mimeType};base64,{base64Content}";
        }

        ArgumentNullException.ThrowIfNull(dataUrl, "dataUrl not set.");

        var chatGpt = kernel.Services.GetService<IChatCompletionService>();

        // allow for prompt overrides
        var imageTextExtractionSystemPrompt = toolDefinition.GetToolPrompt(taskRequest, "ImageTextExtractionSystemPrompt");
        var imageTextExtractionUserPrompt = toolDefinition.GetToolPrompt(taskRequest, "ImageTextExtractionUserPrompt");

        var chatHistory = new ChatHistory(imageTextExtractionSystemPrompt);
        var chatMessageContentItemCollection = new ChatMessageContentItemCollection();
        chatMessageContentItemCollection.Add(new TextContent(imageTextExtractionUserPrompt));
        DataUriParser parser = new DataUriParser(dataUrl);
        chatMessageContentItemCollection.Add(new ImageContent(parser.Data, parser.MediaType));

        chatHistory.AddUserMessage(chatMessageContentItemCollection);

        var result = await chatGpt.GetChatMessageContentAsync(chatHistory, DefaultSettings.AIChatRequestSettings, kernel);

        arguments[$"{file.Name}Text"] = result.Content;
    }

    [KernelFunction("LLMNamedEntityExtraction"), Description("Extract Named Values From Image")]
    public async Task LLMNamedEntityExtractionAsync(KernelArguments arguments, Kernel kernel)
    {
        var agentDefinition = arguments[ContextVariableOptions.AgentDefinition] as AgentDefinition;
        var toolDefinition = arguments[ContextVariableOptions.ToolDefinition] as ToolDefinition;
        var taskRequest = arguments[ContextVariableOptions.TaskRequest] as TaskRequest;
        var imageText = arguments[$"{toolDefinition.RAGSettings.RequestFileName}Text"] as string;
        var agentLog = arguments[ContextVariableOptions.AgentLog] as AgentLog;

        ArgumentNullException.ThrowIfNull(toolDefinition, "toolDefinition not set.");
        ArgumentNullException.ThrowIfNull(taskRequest, "taskRequest not set.");
        ArgumentNullException.ThrowIfNull(imageText, "imageText not set.");
        ArgumentNullException.ThrowIfNull(agentLog, "agentLog not set.");

        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        var sw = Stopwatch.StartNew();

        // allow for prompt overrides
        var imageEntityExtractionSystemPrompt = toolDefinition.GetToolPrompt(taskRequest, "ImageEntityExtractionSystemPrompt");
        var imageEntityExtractionUserPrompt = toolDefinition.GetToolPrompt(taskRequest, "ImageEntityExtractionUserPrompt");

        var chatHistory = new ChatHistory(imageEntityExtractionSystemPrompt);
        var userMessage = await PromptService.RenderPromptAsync(kernel, imageEntityExtractionUserPrompt, arguments);
        chatHistory.AddUserMessage(userMessage);

        var result = await chatGpt.GetChatMessageContentAsync(chatHistory, DefaultSettings.JsonResponseSettings, kernel);
        var labelProperties = JsonSerializer.Deserialize<LabelProperties>(result.Content);

        arguments["LabelPropertiesText"] = result.Content;
        arguments["LabelProperties"] = labelProperties;

        sw.Stop();
        agentLog.LogAgentStep(agentDefinition.Name, toolDefinition.Name, result.Content, new AgentStepDiagnostics(sw.ElapsedMilliseconds));
    }

    [KernelFunction("ExtractConditions"), Description("Extract conditions from provided text")]
    public async Task ExtractConditionsAsync(KernelArguments arguments, Kernel kernel)
    {
        var agentDefinition = arguments[ContextVariableOptions.AgentDefinition] as AgentDefinition;
        var toolDefinition = arguments[ContextVariableOptions.ToolDefinition] as ToolDefinition;
        var taskRequest = arguments[ContextVariableOptions.TaskRequest] as TaskRequest;
        var agentLog = arguments[ContextVariableOptions.AgentLog] as AgentLog;

        ArgumentNullException.ThrowIfNull(toolDefinition, "toolDefinition not set.");
        ArgumentNullException.ThrowIfNull(taskRequest, "taskRequest not set.");
        ArgumentNullException.ThrowIfNull(toolDefinition.RAGSettings, "RAGSettings not set.");
        ArgumentNullException.ThrowIfNull(agentLog, "agentLog not set.");

        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        var sw = Stopwatch.StartNew();

        // allow for prompt overrides
        var systemPrompt = toolDefinition.GetToolPrompt(taskRequest, "EPAExtractConditionsSystemPrompt");
        var userPrompt = toolDefinition.GetToolPrompt(taskRequest, "EPAExtractConditionsUserPrompt");
        if (arguments.ContainsName("Topic"))
           arguments["Topic"] = toolDefinition.RAGSettings.Topic;
        else
           arguments.Add("Topic", toolDefinition.RAGSettings.Topic);

        var chatHistory = new ChatHistory(systemPrompt);
        var userMessage = await PromptService.RenderPromptAsync(kernel, userPrompt, arguments);
        chatHistory.AddUserMessage(userMessage);

        var result = await chatGpt.GetChatMessageContentAsync(chatHistory, DefaultSettings.AIChatRequestSettings, kernel);

        arguments[$"EPARegistration{toolDefinition.RAGSettings.Topic}Summary"] = result.Content;
        arguments[$"EPARegistrationTopicSummary"] = result.Content;
        
        sw.Stop();
        agentLog.LogAgentStep(agentDefinition.Name, toolDefinition.Name, result.Content, new AgentStepDiagnostics(sw.ElapsedMilliseconds));
    }

    [KernelFunction("ExtractFullConditions"), Description("Extract conditions from provided text")]
    public async Task ExtractFullConditionsAsync(KernelArguments arguments, Kernel kernel)
    {
        var agentDefinition = arguments[ContextVariableOptions.AgentDefinition] as AgentDefinition;
        var toolDefinition = arguments[ContextVariableOptions.ToolDefinition] as ToolDefinition;
        var taskRequest = arguments[ContextVariableOptions.TaskRequest] as TaskRequest;
        var agentLog = arguments[ContextVariableOptions.AgentLog] as AgentLog;

        ArgumentNullException.ThrowIfNull(toolDefinition, "toolDefinition not set.");
        ArgumentNullException.ThrowIfNull(taskRequest, "taskRequest not set.");
        ArgumentNullException.ThrowIfNull(agentLog, "agentLog not set.");

        var chatGpt = kernel.Services.GetService<IChatCompletionService>();
        var sw = Stopwatch.StartNew();

        // allow for prompt overrides
        var systemPrompt = toolDefinition.GetToolPrompt(taskRequest, "EPAExtractFullConditionsSystemPrompt");
        var userPrompt = toolDefinition.GetToolPrompt(taskRequest, "EPAExtractFullConditionsUserPrompt");

        var chatHistory = new ChatHistory(systemPrompt);
        var userMessage = await PromptService.RenderPromptAsync(kernel, userPrompt, arguments);
        chatHistory.AddUserMessage(userMessage);

        var result = await chatGpt.GetChatMessageContentAsync(chatHistory, DefaultSettings.AIChatRequestSettings, kernel);

        arguments[$"EPARegistrationTopicSummary"] = result.Content;

        sw.Stop();
        agentLog.LogAgentStep(agentDefinition.Name, toolDefinition.Name, result.Content, new AgentStepDiagnostics(sw.ElapsedMilliseconds));
    }
}