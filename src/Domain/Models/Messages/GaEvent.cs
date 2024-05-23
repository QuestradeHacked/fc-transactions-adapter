using Newtonsoft.Json;

namespace Domain.Models.Messages;

public class GaEvent
{
    [JsonProperty("APIEventName")]
    public string? ApiEventName { get; set; }
    public XceedCommon? Common { get; set; }
}
