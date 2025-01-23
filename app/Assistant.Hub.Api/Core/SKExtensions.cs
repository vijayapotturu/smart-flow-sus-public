using Assistant.Hub.Api.Services.Profile.Prompts;
using Assistants.Hub.API.Core;
using Azure.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Assistants.API.Core
{
    public static class SKExtensions
    {
        public static ChatHistory BuildChatHistory(this TaskRequest taskRequest, string systemPrompt, string userPrompt)
        {
            var chatHistory = new ChatHistory(systemPrompt);
            var chatMessageContentItemCollection = new ChatMessageContentItemCollection();
            chatMessageContentItemCollection.Add(new TextContent(userPrompt));

            foreach (var file in taskRequest.Files)
            {
                if (!string.IsNullOrEmpty(file.DataUrl)) 
                { 
                    DataUriParser parser = new DataUriParser(file.DataUrl);
                    if (parser.MediaType == "image/jpeg" || parser.MediaType == "image/png")
                    {
                        chatMessageContentItemCollection.Add(new ImageContent(parser.Data, parser.MediaType));
                    }
                }
                //TODO: Handle invalid files
                //TODO: Piotr - handle other types of files (json, txt)
            }
            chatHistory.AddUserMessage(chatMessageContentItemCollection);

            return chatHistory;
        }
    }
}
