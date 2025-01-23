using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Assistant.Hub.Api.Batch.Functions
{
    public class HealthCheck
    {
        [Function("HealthCheck")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="health")] HttpRequest req)
        {
            return new OkResult();
        }
    }
}
