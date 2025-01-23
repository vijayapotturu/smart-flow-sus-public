namespace Assistants.API.Core
{
    public static class AppConfigurationSetting
    {
        public static string AOAIStandardChatGptDeployment => "AOAIStandardChatGptDeployment";
        public static string AOAIStandardServiceEndpoint => "AOAIStandardServiceEndpoint";
        public static string AOAIStandardServiceKey => "AOAIStandardServiceKey";

        public static string AzureAISearchEndpoint => "AzureAISearchEndpoint";

        public static string AzureAISearchKey => "AzureAISearchKey";

        public static string AzureDocumentIntelligenceKey => "AzureDocumentIntelligenceKey";
        public static string AzureDocumentIntelligenceEndpoint => "AzureDocumentIntelligenceEndpoint";

        public static string CosmosDbEndpoint => "CosmosDbEndpoint";
        public static string CosmosDbKey => "CosmosDbKey";

        public static string StorageAccountName { get; } = "StorageAccountName";
        public static string ContentStorageContainer { get; } = "ContentStorageContainer";
    }
}
