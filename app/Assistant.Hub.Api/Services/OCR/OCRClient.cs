using Assistant.Hub.Api.Assistants;
using Assistants.API.Core;
using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Storage.Blobs;

namespace Assistant.Hub.Api.Services.OCR
{
    public class OcrClient
    {
        private readonly DocumentIntelligenceClient _documentIntelligenceClient;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly ILogger<OcrClient> _logger;

        public OcrClient(DocumentIntelligenceClient documentIntelligenceClient, BlobContainerClient blobContainerClient, ILogger<OcrClient> logger)
        {
            _documentIntelligenceClient = documentIntelligenceClient;
            _blobContainerClient = blobContainerClient;
            _logger = logger;
        }

        public async Task<string> ExtractAsMarkdownFromDataUrlAsync(string dataUrl, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Extracting markdown content from {dataUrl}", dataUrl);
            byte[] imageBytes = ExtractBytesFromDataUrl(dataUrl);
            _logger.LogDebug("Extracted {bytes} bytes from {dataUrl}", imageBytes.Length, dataUrl);

            var options = new AnalyzeDocumentOptions("prebuilt-layout", BinaryData.FromBytes(imageBytes)) { OutputContentFormat = "markdown" };
            Operation<AnalyzeResult> operation = await _documentIntelligenceClient.AnalyzeDocumentAsync(WaitUntil.Completed, options, cancellationToken);

            _logger.LogInformation("DocumentIntelligence extracted markdown content. Pages: {pageCount}", operation.Value.Pages.Count);

            foreach (var warning in operation.Value.Warnings)
            {
                _logger.LogWarning("Warning: {warningCode} - {warningMessage}", warning.Code, warning.Message);
            }

            AnalyzeResult result = operation.Value;
            return result.Content;
        }

        public async Task<string?> ExtractAsMarkdownAsync(RequestFile file, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Extracting markdown content from file {file}", file.DataUrl);
            var dataUrl = file.GetDataUrlForPngJpeg();

            if (!string.IsNullOrEmpty(dataUrl))
            {
                _logger.LogInformation("Extracting markdown from jpg/png");
                var content = await ExtractAsMarkdownFromDataUrlAsync(dataUrl, cancellationToken);
                return content;
            }

            var blobName = file.GetBlobName();
            if (blobName != null)
            {
                _logger.LogInformation("Extracting markdown from blob {blobName}", blobName);
                var content = await ExtractAsMarkdownFromBlobAsync(blobName, cancellationToken);
                return content;
            }

            _logger.LogWarning("No valid file type provided");
            return null;
        }

        public async Task<string> ExtractAsMarkdownFromBlobAsync(string blobName, CancellationToken cancellationToken = default)
        {
            var imageBytes = await GetBlobContentAsync(blobName, cancellationToken);

            _logger.LogInformation("Extracting markdown content usign DocInteli from blob {blobName} using 'prebuilt-layout' model", blobName);
            var options = new AnalyzeDocumentOptions("prebuilt-layout", BinaryData.FromBytes(imageBytes)) { OutputContentFormat = "markdown" };

            Operation<AnalyzeResult> operation = await _documentIntelligenceClient.AnalyzeDocumentAsync(WaitUntil.Completed, options, cancellationToken);
            AnalyzeResult result = operation.Value;

            foreach (var warning in operation.Value.Warnings)
            {
                _logger.LogWarning("Warning: {warningCode} - {warningMessage}", warning.Code, warning.Message);
            }

            _logger.LogInformation("DocumentIntelligence extracted markdown content. Pages: {pageCount}", result.Pages.Count);
            return result.Content;
        }

        private static byte[] ExtractBytesFromDataUrl(string dataUrl)
        {
            // 1. Find the index of the comma that separates the metadata from the Base64 data.
            int commaIndex = dataUrl.IndexOf(',');

            // 2. Extract the Base64 portion of the string (everything after the comma).
            string base64Data = dataUrl.Substring(commaIndex + 1);

            // 3. Decode the Base64 string into a byte array.
            return Convert.FromBase64String(base64Data);
        }

        public async Task<BinaryData> GetBlobContentAsync(string blobName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving blob content from {blobName}", blobName);

            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            var blobDownloadInfo = await blobClient.DownloadContentAsync(cancellationToken);

            _logger.LogInformation("Retrieved {bytes} bytes from {blobName}", blobDownloadInfo.Value.Details.ContentLength, blobName);
            return blobDownloadInfo.Value.Content;
        }
    }
}
