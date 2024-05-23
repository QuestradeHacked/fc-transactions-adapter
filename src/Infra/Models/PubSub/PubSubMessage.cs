using Newtonsoft.Json;

namespace Infra.Models.PubSub;

public class PubSubMessage<TData>
{
    [JsonProperty("data")]
    public TData? Data { get; set; }

    [JsonProperty("dataContentType")]
    public string? DataContentType { get; set; }

    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("source")]
    public string? Source { get; set; }

    [JsonProperty("specVersion")]
    public string? SpecVersion { get; set; }

    [JsonProperty("time")]
    public DateTime? Time { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }
}
