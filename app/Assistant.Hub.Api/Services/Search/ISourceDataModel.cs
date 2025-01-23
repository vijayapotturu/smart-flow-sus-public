namespace Assistant.Hub.Api.Services.Search
{
    public interface ISourceDataModel    {
        string FormatAsOpenAISourceText(bool useSourcepage = false);
        string GetFilepath(bool useSourcepage = false);
        string GetContent();
    }
}
