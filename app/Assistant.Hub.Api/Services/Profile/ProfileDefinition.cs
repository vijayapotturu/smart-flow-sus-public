using Newtonsoft.Json;
using System.Reflection;
using System.Text;

namespace Assistant.Hub.Api.Services.Profile
{

    // TODO: PKA - add ProfileProvider a singleton and add to DI
    public class ProfileDefinition
    {
        public static List<ProfileDefinition> All { get; } = LoadProflies("profiles");

        private static List<ProfileDefinition> LoadProflies(string name)
        {
            var resourceName = $"Assistant.Hub.Api.Services.Profile.{name}.json";
            var assembly = Assembly.GetExecutingAssembly();

            using Stream stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new ArgumentException($"The resource {resourceName} was not found.");

            List<ProfileDefinition> profiles = System.Text.Json.JsonSerializer.Deserialize<List<ProfileDefinition>>(stream)
                ?? throw new ArgumentException($"The resource {resourceName} could not be deserialized.");

            return profiles;
        }

        public ProfileDefinition(string description, string name, List<AgentDefinition> agents, string format)
        {
            Name = name;
            Description = description;
            Agents = agents;
            Format = format;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Format { get; set; }

        public List<AgentDefinition> Agents { get; set; }
    }

    public record AgentDefinition(string Name, string Type, string SystemMessage, string UserMessage, ReviewSettings? ReviewerSettings, IEnumerable<ToolDefinition>? Tools);
    public record ToolDefinition(string Name, string Function, string Type, RAGSettingsSummary? RAGSettings, Dictionary<string,string>? Settings);
    public record ReviewSettings(string SystemMessage, string UserMessage, int MaxReviewAttempts = 1);

    public class RAGSettingsSummary
    {
        public  string? RetrievalPluginQueryFunctionName { get; set; }
        public  string? RetrievalIndexName { get; set; }

        public  int DocumentRetrievalDocumentCount { get; set; }
        public  int DocumentRetrievalMaxSourceTokens { get; set; } = 12000;

        public  bool CitationUseSourcePage { get; set; }

        public  int KNearestNeighborsCount { get; set; } = 3;

        public  string EmbeddingsDeployment { get; set; } = "TextEmbeddings";

        public  string? VectorSearchQurey { get; set; }

        public  string? DataFileName { get; set; }

        public string? RequestFileName { get; set; }

        public string? Topic { get; set; }
    }
}
