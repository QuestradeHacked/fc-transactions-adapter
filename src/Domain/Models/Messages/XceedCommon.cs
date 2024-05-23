using Newtonsoft.Json;

namespace Domain.Models.Messages;

public class XceedCommon
{
    public string? Channel { get; set; }

    public string? ProviderEventName { get; set; }

    public string? SchemaVersion { get; set; }

    [JsonProperty("TenantID")]
    public string? TenantId { get; set; }
}
