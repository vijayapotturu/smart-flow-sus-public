// Copyright (c) Microsoft. All rights reserved.

using Assistant.Hub.Api.Core;
using Assistant.Hub.Api.Services.OCR;
using Assistant.Hub.Api.Services.Profile;
using Assistants.API.Core;
using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Assistant.Hub.Api.Services.Search;

public sealed class DocumentIntelligenceExtractionTools
{
    private readonly OcrClient _ocrClient;
    private readonly BlobContainerClient _blobContainerClient;
    private readonly ILogger<DocumentIntelligenceExtractionTools> _logger;

    public DocumentIntelligenceExtractionTools(OcrClient ocrClient, BlobContainerClient blobContainerClient, ILogger<DocumentIntelligenceExtractionTools> logger)
    {
        _ocrClient = ocrClient;
        _blobContainerClient = blobContainerClient;
        _logger = logger;
    }


    [KernelFunction("PDFTextExtraction"), Description("Extract text from PDF")]
    public async Task TaskTextExtractionAsync(KernelArguments arguments, CancellationToken cancellationToken)
    {
        var agentDefinition = arguments[ContextVariableOptions.AgentDefinition] as AgentDefinition;
        var toolDefinition = arguments[ContextVariableOptions.ToolDefinition] as ToolDefinition;
        var taskRequest = arguments[ContextVariableOptions.TaskRequest] as TaskRequest;
        var agentLog = arguments[ContextVariableOptions.AgentLog] as AgentLog;

        ArgumentNullException.ThrowIfNull(toolDefinition, "toolDefinition not set.");
        ArgumentNullException.ThrowIfNull(toolDefinition.RAGSettings?.RequestFileName, "toolDefinition.RAGSettings.RequestFileName not set.");
        ArgumentNullException.ThrowIfNull(taskRequest, "taskRequest not set.");
        ArgumentNullException.ThrowIfNull(agentLog, "agentLog not set.");
        var sw = Stopwatch.StartNew();

        var file = taskRequest.Files.FirstOrDefault(x => x.Name == toolDefinition.RAGSettings.RequestFileName)
            ?? throw new KeyNotFoundException($"file '{toolDefinition.RAGSettings.RequestFileName}' not found in the request");

        var content = await ExecuteWithCache(file, cancellationToken);
        if (string.IsNullOrEmpty(content))
        {
            throw new InvalidOperationException($"No content extracted from the file");
        }

        arguments[$"{toolDefinition.RAGSettings.RequestFileName}Text"] = content;

        sw.Stop();
        agentLog.LogAgentStep(agentDefinition.Name, toolDefinition.Name, content, new AgentStepDiagnostics(sw.ElapsedMilliseconds));
    }

    private async Task<string?> ExecuteWithCache(RequestFile file, CancellationToken cancellationToken)
    {
        var hashName = ComputeMD5Hash(file.DataUrl);
        var blobClient = _blobContainerClient.GetBlobClient($"{hashName}.md");
        var exists = await blobClient.ExistsAsync();
        if (!exists)
        {
            var content = await _ocrClient.ExtractAsMarkdownAsync(file, cancellationToken);
            using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            return content;
        }
        var blobDownloadInfo = await blobClient.DownloadContentAsync();
        string stringValue = blobDownloadInfo.Value.Content.ToString();
        return stringValue;
    }

    private static string ComputeMD5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes); // Directly converts bytes to hex
        }
    }
}
