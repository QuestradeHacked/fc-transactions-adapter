using Newtonsoft.Json;

namespace Domain.Models.Messages;

public class SearchMatch
{
    [JsonProperty("searchMatch")]
    public string? Name { get; set; }
}
