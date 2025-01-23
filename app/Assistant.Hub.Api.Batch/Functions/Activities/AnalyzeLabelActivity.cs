using Assistant.Hub.Api.Batch.Models;
using Assistant.Hub.Api.Batch.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Assistant.Hub.Api.Batch.Functions.Activities;

public  class AnalyzeLabelActivity
{
    private readonly AssistantApiHttpClient _client;

    public AnalyzeLabelActivity(AssistantApiHttpClient client)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        _client = client;
    }

    [Function(nameof(AnalyzeLabelActivity))]
    public async Task<LabelAnalysisResult> RunAsync([ActivityTrigger] TaskRequest request,
        FunctionContext context)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));
        LabelAnalysisResult result;
        var logger = context.GetLogger(nameof(AnalyzeLabelActivity));

        try
        {
            result = await _client.AnalyzeLabelAsync(request);
        }
        catch (Exception e)
        {
            logger.LogError(e, $"Error calling Assistant API: {e.Message}");
            throw;
        }

        return result;
    }
}
