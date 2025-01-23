using Microsoft.SemanticKernel;

namespace Assistants.API.Core
{
    public static class DefaultSettings
    {
        public static PromptExecutionSettings AIChatRequestSettings = new()
        {
            ExtensionData = new Dictionary<string, object>()
            {
                { "max_tokens", 2048 },
                { "temperature", 0.0 },
                { "top_p", 1 },
            }
        };

        public static PromptExecutionSettings JsonResponseSettings = new()
        {
            ExtensionData = new Dictionary<string, object>()
            {
                { "max_tokens", 2048 },
                { "temperature", 0.0 },
                { "top_p", 1 },
                { "response_format", "json_object" }
            }
        };
    }
}
