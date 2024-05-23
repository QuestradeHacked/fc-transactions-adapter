using Newtonsoft.Json;

namespace Domain.Models.Messages;

public class RiskFactor
{
    [JsonProperty("riskFactor")]
    public string? Name { get; set; }
}
