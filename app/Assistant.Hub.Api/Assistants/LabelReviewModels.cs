using Assistants.API.Core;

namespace Assistant.Hub.Api.Assistants
{
    public record LabelProperties(string product_name, string regulator_number, string country, string upc, string classification);
    public record ReviewerResponse(bool complete, string feedback);
}
