using Azure.Identity;
using Azure.Storage;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Assistant.Hub.Api.Batch;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddCosmosDbClient(this IServiceCollection services, IConfiguration configuration)
    {
        var key = configuration[ConfigurationKeys.CosmosDbKey];
        var cosmosDbEndpoint = configuration[ConfigurationKeys.CosmosDbEndpoint];

        ArgumentException.ThrowIfNullOrWhiteSpace(cosmosDbEndpoint, "Cosmos DB endpoint is not configured");

        var options = new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,
            
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };

        if (string.IsNullOrWhiteSpace(key))
        {
            services.AddSingleton(sp => new CosmosClient(cosmosDbEndpoint, new DefaultAzureCredential(new DefaultAzureCredentialOptions{ ExcludeVisualStudioCredential = true}), options));
        }
        else
        {
            services.AddSingleton(sp => new CosmosClient(cosmosDbEndpoint, key, options));
        }

        return services;
    }

    public static AzureClientFactoryBuilder AddBatchAnalysisBlobServiceClient(this AzureClientFactoryBuilder builder, IConfiguration configuration)
    {
        var key = configuration[ConfigurationKeys.BatchAnalysisStorageAccountKey];
        var accountName = configuration[ConfigurationKeys.BatchAnalysisStorageAccountName];
        var container = configuration[ConfigurationKeys.BatchAnalysisStorageInputContainerName];

        ArgumentException.ThrowIfNullOrWhiteSpace(accountName, "Storage account name is not configured");
        ArgumentException.ThrowIfNullOrWhiteSpace(container, "Storage container is not configured");

        if (string.IsNullOrWhiteSpace(key))
        {
            builder.AddBlobServiceClient(new Uri($"https://{accountName}.blob.core.windows.net/")).WithCredential(new DefaultAzureCredential(new DefaultAzureCredentialOptions{ExcludeVisualStudioCredential = true}));
        }
        else
        {
            builder.AddBlobServiceClient(new Uri($"https://{accountName}.blob.core.windows.net/"), new StorageSharedKeyCredential(accountName, key));
        }

        return builder;
    }

    public static IServiceCollection AddBatchOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddSingleton<IOpenApiConfigurationOptions>(sp =>
        {
            var options = new OpenApiConfigurationOptions
            {
                Info = new OpenApiInfo
                {
                    Version = "1.0.0",
                    Title = "Assistant Batch Analysis API",
                    Description = "Assistant Batch Analysis API"
                },
                Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
                OpenApiVersion = DefaultOpenApiConfigurationOptions.GetOpenApiVersion(),
                IncludeRequestingHostName = DefaultOpenApiConfigurationOptions.IsFunctionsRuntimeEnvironmentDevelopment()
            };

            return options;
        });

        return services;
    }
}