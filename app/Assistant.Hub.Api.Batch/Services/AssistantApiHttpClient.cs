using System.Net.Http.Json;
using Assistant.Hub.Api.Batch.Models;
using Microsoft.Extensions.Configuration;

namespace Assistant.Hub.Api.Batch.Services;

public class AssistantApiHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _profileName;

    public AssistantApiHttpClient(HttpClient httpClient,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        _httpClient = httpClient;

        var profile = configuration[ConfigurationKeys.AnalysisApiProfileName];
        ArgumentException.ThrowIfNullOrWhiteSpace(profile);
        _profileName = profile;
    }

    public async Task<LabelAnalysisResult> AnalyzeLabelAsync(TaskRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var response = await _httpClient.PostAsJsonAsync(_profileName, request);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();

        return new LabelAnalysisResult("", "", "", responseJson);
    }
}
