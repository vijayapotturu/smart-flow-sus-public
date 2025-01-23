using Microsoft.SemanticKernel;
using System.Reflection;
using Assistants.API.Core;

namespace Assistant.Hub.Api.Services.Profile.Prompts
{
    public static class PromptService
    {
        //public static string WeatherUserPrompt = "WeatherUserPrompt";
        //public static string ChatUserPrompt = "RAGChatUserPrompt";
        //public static string RAGSearchSystemPrompt = "RAGSearchQuerySystemPrompt";
        //public static string RAGSearchUserPrompt = "RAGSearchUserPrompt";

        //public static string ChatSimpleSystemPrompt = "ChatSimpleSystemPrompt";
        //public static string ChatSimpleUserPrompt = "ChatSimpleUserPrompt";

        private static string GetPromptByName(string prompt)
        {
            var resourceName = $"Assistant.Hub.Api.Services.Profile.Prompts.{prompt}.txt";
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new ArgumentException($"The resource {resourceName} was not found.");

                using (StreamReader reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }

        public static async Task<string> RenderPromptAsync(Kernel kernel, string prompt, KernelArguments arguments)
        {
            var ptf = new KernelPromptTemplateFactory();
            var pt = ptf.Create(new PromptTemplateConfig(prompt));
            string intentUserMessage = await pt.RenderAsync(kernel, arguments);
            return intentUserMessage;
        }

        public static string ResolvePrompt(string prompt, TaskRequest request)
        {
            if (!prompt.Contains("::"))
                return PromptService.GetPromptByName(prompt);

            var parts = prompt.Split("::", StringSplitOptions.TrimEntries);
            if (parts[0] == "inline")
            {
                return parts[1];
            }

            if (parts[0] == "embeddedFile")
            {
                return PromptService.GetPromptByName(parts[1]);
            }

            if (parts[0] == "request")
            {
                var prompts = request.prompts ?? throw new InvalidOperationException("Prompts were not provided in the request");
                return prompts.TryGetValue(parts[1], out var promptText)
                    ? promptText
                    : throw new KeyNotFoundException($"Prompt with key '{parts[1]}' was not provided in the request");
            }

            throw new InvalidOperationException($"Invalid prompt configuration: '{prompt}'. Use one of prefixes: inline::, embeddedFile:: or request:: to configure the prompt");
        }
    }
}
