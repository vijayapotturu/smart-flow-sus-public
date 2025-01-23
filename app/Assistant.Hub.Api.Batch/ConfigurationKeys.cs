namespace Assistant.Hub.Api.Batch;

public static class ConfigurationKeys
{
    public static string MaxBatchSize => "MaxBatchSize";

    public static string BatchAnalysisStorageAccountName => "BatchAnalysisStorageAccountName";

    public static string BatchAnalysisStorageAccountKey => "BatchAnalysisStorageAccountKey";

    public static string BatchAnalysisStorageInputContainerName => "BatchAnalysisStorageInputContainerName";

    public static string BatchAnalysisStorageOutputContainerName => "BatchAnalysisStorageOutputContainerName";

    public static string CosmosDbKey => "CosmosDbKey";

    public static string CosmosDbEndpoint => "CosmosDbEndpoint";

    public static string CosmosDbDatabaseName => "CosmosDbDatabaseName";

    public static string CosmosDbContainerName => "CosmosDbContainerName";

    public static string AnalysisApiEndpoint => "AnalysisApiEndpoint";

    public static string AnalysisApiKey => "AnalysisApiKey";

    public static string AnalysisApiProfileName => "AnalysisApiProfileName";
}