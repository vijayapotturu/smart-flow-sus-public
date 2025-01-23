using System.Net;
using Assistant.Hub.Api.Batch.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Assistant.Hub.Api.Batch.Functions
{
    public class SubmitLabelBatchForAnalysis
    {
        private readonly ILogger<SubmitLabelBatchForAnalysis> _logger;
        private readonly int _maxBatchSize;

        public SubmitLabelBatchForAnalysis(ILogger<SubmitLabelBatchForAnalysis> logger, IConfiguration configuration)
        {
            _logger = logger;

            if (!int.TryParse(configuration[ConfigurationKeys.MaxBatchSize], out _maxBatchSize))
            {
                _maxBatchSize = 10; // Hard coded default
            }
        }

        [Function("SubmitLabelBatchForAnalysis")]
        [OpenApiOperation(operationId: "SubmitLabelBatchForAnalysis")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(List<TaskRequest>), Required = true, Description = "The list of labels to process")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BatchAccepted), Description = "The batch has been accepted for processing")]
        [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            [DurableClient] DurableTaskClient durableTaskClient)
        {
            IActionResult result;

            try
            {
                var batchRequest = await req.ReadFromJsonAsync<List<TaskRequest>>();
                ArgumentNullException.ThrowIfNull(batchRequest, nameof(batchRequest));

                if (batchRequest.Count == 0)
                {
                    result = new BadRequestObjectResult("No labels to process");
                }
                else if (batchRequest.Count > _maxBatchSize)
                {
                    result = new BadRequestObjectResult($"Batch size exceeds maximum");
                }
                else
                {
                    var instanceId = await durableTaskClient.ScheduleNewOrchestrationInstanceAsync(nameof(LabelBatchAnalysisOrchestration), batchRequest);

                    BatchAccepted accepted = new(instanceId);

                    var locationPath = $"/batchstatus/{instanceId}";

                    result = new AcceptedResult(locationPath, accepted);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing request: {ex.Message}");
                result = new StatusCodeResult(500);
            }

            return result;
        }
    }
}
