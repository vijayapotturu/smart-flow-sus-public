namespace Assistant.Hub.Api.Services.Search
{
    public class VectorSearchResult
    {
        public VectorSearchResult(string formattedSourceText, IEnumerable<ISourceDataModel> sources)
        {
            FormattedSourceText = formattedSourceText;
            Sources = sources;
        }

        public string FormattedSourceText { get; set; }

        public IEnumerable<ISourceDataModel> Sources { get; set; }
    }

    public record ContentSearchResult(string id, string content);
}
