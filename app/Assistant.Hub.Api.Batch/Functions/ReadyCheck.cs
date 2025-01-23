using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Assistant.Hub.Api.Batch.Functions;

public class ReadyCheck
{
    [Function("ReadyCheck")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ready")] HttpRequest req)
    {
        return new OkResult();
    }
}