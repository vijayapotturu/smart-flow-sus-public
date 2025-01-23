using Assistant.Hub.Api.Services.OCR;
using Assistants.API.Core;
using Azure.Storage.Blobs;

public class ContentService
{
    private readonly ILogger<ContentService> _logger;

    private readonly OcrClient _ocrClient;

    private readonly BlobContainerClient _blobContainerClient;

    public ContentService(ILogger<ContentService> logger, OcrClient ocrClient, BlobContainerClient blobContainerClient)
    {
        _logger = logger;
        _ocrClient = ocrClient;
        _blobContainerClient = blobContainerClient;
    }

    public async Task<string> ResolveFileContent(IndexDocumentRequest request, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(request.blobName) && request.blobName.EndsWith(".md"))
        {
            _logger.LogInformation("Retrieving markdown content from blob {blobName}", request.blobName);
            var blobClient = _blobContainerClient.GetBlobClient(request.blobName);
            var blobDownloadInfo = await blobClient.DownloadContentAsync(cancellationToken);

            if (blobDownloadInfo.Value.Content == null)
            {
                throw new InvalidOperationException($"Blob {request.blobName} content is null.");
            }

            string stringValue = blobDownloadInfo.Value.Content.ToString();
            return stringValue;
        }

        if (!string.IsNullOrEmpty(request.dataUrl))
        {
            _logger.LogInformation("Retrieving markdown content using  DocumentIntelligence from data url {dataUrl}", request.dataUrl);
            var content = await _ocrClient.ExtractAsMarkdownFromDataUrlAsync(request.dataUrl, cancellationToken);
            return content;
        }

        if (!string.IsNullOrEmpty(request.blobName))
        {
            _logger.LogInformation("Retrieving markdown content using DocumentIntelligence from blob {blobName}", request.blobName);
            var content = await _ocrClient.ExtractAsMarkdownFromBlobAsync(request.blobName, cancellationToken);
            return content;
        }

        throw new InvalidOperationException("Valid file type not provided.");
    }

    public async Task<string[]> ResolveFileContentV2(IndexDocumentRequest request, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(request.dataUrl))
        {
            var content = await _ocrClient.ExtractAsMarkdownFromDataUrlAsync(request.dataUrl, cancellationToken);
            var chunks = content.Split("##");
            return chunks;
        }

        if (!string.IsNullOrEmpty(request.blobName))
        {
            var content = await _ocrClient.ExtractAsMarkdownFromBlobAsync(request.blobName, cancellationToken);
            var chunks = content.Split("##");
            return chunks;
        }

        throw new InvalidOperationException("Valid file type not provided.");
    }
}