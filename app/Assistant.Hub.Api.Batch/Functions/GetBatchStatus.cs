using System.Net;
using Assistant.Hub.Api.Batch.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Assistant.Hub.Api.Batch.Functions
{
    public class GetBatchStatus
    {
        private readonly ILogger<GetBatchStatus> _logger;
        private readonly CosmosClient _cosmosCosmosClient;
        private readonly string _cosmosDbName;
        private readonly string _containerName;

        public GetBatchStatus(ILogger<GetBatchStatus> logger,
            CosmosClient cosmosClient,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(cosmosClient, nameof(cosmosClient));

            var cosmosDbName = configuration[ConfigurationKeys.CosmosDbDatabaseName];
            var containerName = configuration[ConfigurationKeys.CosmosDbContainerName];

            ArgumentException.ThrowIfNullOrWhiteSpace(cosmosDbName);
            ArgumentException.ThrowIfNullOrWhiteSpace(containerName);

            _logger = logger;
            _cosmosCosmosClient = cosmosClient;
            _cosmosDbName = cosmosDbName;
            _containerName = containerName;
        }

        [Function(nameof(GetBatchStatus))]
        [OpenApiOperation(nameof(GetBatchStatus))]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter("batchId", Type=typeof(string), In = ParameterLocation.Path)]
        [OpenApiResponseWithBody(System.Net.HttpStatusCode.OK, "application/json", typeof(BatchStatus))]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound)]
        [OpenApiResponseWithoutBody(HttpStatusCode.TooManyRequests)]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof(ApiError))]
        public async  Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, 
            "get",
            Route = "batchstatus/{batchId}")] HttpRequest req,
            string batchId)
        {
            IActionResult result;

            try
            {
                _logger.LogInformation($"Retrieving batch status for {batchId}");

                var doc = await _cosmosCosmosClient.GetContainer(_cosmosDbName, _containerName)
                    .ReadItemAsync<BatchStatus>(batchId, new PartitionKey(batchId));

                _logger.LogInformation($"Successfully retrieved status for batch {batchId}");

                result = new OkObjectResult(doc.Resource);
            }
            catch (CosmosException e)
            {
                _logger.LogError(e, $"Exception when querying status: {e.Message}");

                result = e.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => new NotFoundResult(),
                    System.Net.HttpStatusCode.TooManyRequests => new StatusCodeResult(429),
                    _ => new BadRequestObjectResult(new ApiError{ Message = e.Message})
                };
            }

            return result;
        }
    }
}
