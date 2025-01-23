using System.Text.Json;
using System.Text.Json.Serialization;
using Assistant.Hub.Api.Batch;
using Assistant.Hub.Api.Batch.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

 builder.Services.AddApplicationInsightsTelemetryWorkerService()
     .ConfigureFunctionsApplicationInsights();

builder.Services.AddCosmosDbClient(builder.Configuration);

builder.Services.AddAzureClients(clients =>
{
    clients.AddBatchAnalysisBlobServiceClient(builder.Configuration);
});

builder.Services.Configure<JsonSerializerOptions>(opt =>
{
    opt.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

builder.Services.AddHttpClient<AssistantApiHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration[ConfigurationKeys.AnalysisApiEndpoint]);
    client.DefaultRequestHeaders.Add("x-api-key", builder.Configuration[ConfigurationKeys.AnalysisApiKey]);
});

builder.Services.AddBatchOpenApiConfiguration();

builder.Build().Run();