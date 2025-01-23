using Assistant.Hub.Api.Services.Profile;
using Assistants.API.Core;
using Assistant.Hub.Api.Services.Profile.Prompts;

namespace Assistant.Hub.Api.Core;

public static class PromptExtensions
{
    /// <summary>
    /// Get prompt from tool definition
    /// </summary>
    /// <param name="toolDefinition">Tool to get the prompt for</param>
    /// <param name="taskRequest">Overall task request</param>
    /// <param name="settingName">Name of the setting in tool definition that has the prompt</param>
    /// <param name="defaultPrompt">Default prompt to use. Defaults to 'embedded::{settingName}' when missing</param>
    /// <returns>Prompt text</returns>
    public static string GetToolPrompt(this ToolDefinition toolDefinition, TaskRequest taskRequest, string settingName, string defaultPrompt = null)
    {
        var prompt = toolDefinition.Settings != null && toolDefinition.Settings.TryGetValue(settingName, out var promptFromSettings)
            ? promptFromSettings
            : (defaultPrompt ?? $"embeddedFile::{settingName}");

        return PromptService.ResolvePrompt(prompt, taskRequest);
    }
}