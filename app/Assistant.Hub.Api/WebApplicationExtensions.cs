using Assistants.API.Core;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Services;
using Assistant.Hub.Api.Services.Profile;
using Azure.Search.Documents.Indexes;
using Assistant.Hub.Api.Services.Search;

namespace Assistants.API
{
    internal static class WebApplicationExtensions
    {
        internal static WebApplication MapApi(this WebApplication app)
        {
            var api = app.MapGroup("api");
            api.MapPost("task/{workflow}", ProcessLabelAnalysis);

            api.MapPost("admin/index/create", AdminProcessIndexCreation);
            api.MapPost("admin/index/add", AdminProcessIndexAdd);

            api.MapGet("data/product/{upc}", DataProcessProductGet);
            return app;
        }

        private static async Task<IResult> ProcessLabelAnalysis(TaskRequest request, [FromServices] WorkflowManagerAgent workflowManagerAgent, string workflow, [FromServices] ILogger<WorkflowManagerAgent> logger, CancellationToken cancellationToken)
        {
            try
            {
                var profile = ProfileDefinition.All.FirstOrDefault(p => p.Name.Equals(workflow, StringComparison.OrdinalIgnoreCase));

                if (profile == null)
                {
                    return Results.NotFound( $"Profile {workflow} not found");
                }

                logger.LogInformation("Processing request for workflow {workflow}", workflow);

                var result = await workflowManagerAgent.ProcessesRequestAsync(request, profile, cancellationToken: cancellationToken);

                logger.LogInformation("Request for workflow {workflow} processed successfully", workflow);

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing request for workflow {workflow}", workflow);
                return Results.Problem(ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> AdminProcessIndexCreation(IndexRequest request, [FromServices] SearchIndexClient searchIndexClient, [FromServices] ILogger<WebApplication> logger, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Creating index {indexName}", request.indexName);
                await searchIndexClient.EnsureIndexExists(request.indexName, cancellationToken);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating index {indexName}", request.indexName);
                return Results.Problem(ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> AdminProcessIndexAdd(
            IndexDocumentRequest request, 
            [FromServices] SearchClientFactory searchClientFactory, 
            [FromServices] ILogger<WebApplication> logger, 
            [FromServices] ContentService contentService,
            CancellationToken cancellationToken)
        {
            try 
            {
                logger.LogInformation("Creating index {indexName}", request.indexName);
                logger.LogInformation("Tryining to resolve file content using {sourceName} {blobName} and {dataUrl}", request.sourceName, request.blobName, request.dataUrl);
                var content = await contentService.ResolveFileContent(request, cancellationToken);

                logger.LogInformation("Resolved content (length: {contentLength}) for {indexName}", content.Length, request.indexName);
                var searchClient = searchClientFactory.GetOrCreateClient(request.indexName);
                await searchClient.AddDocumentAsync(request.key, content, request.sourceName);

                logger.LogInformation("Document added to index {indexName}", request.indexName);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding document to index {indexName}", request.indexName);
                return Results.Problem(ex.Message, statusCode: 500);
            }
        }

        private static async Task<ProductMasterInformation> DataProcessProductGet(string upc)
        {
            if (upc == "046500014821")
            {
                return new ProductMasterInformation(new[] { new ProductMasterItem("Weight", "Net Wt. 3 oz") });
            }

            return new ProductMasterInformation(new[] { new ProductMasterItem("Weight", "Net Wt. 12 oz") });
        }
    }
}
