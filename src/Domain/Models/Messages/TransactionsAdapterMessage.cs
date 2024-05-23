using Newtonsoft.Json;

namespace Domain.Models.Messages;

public class TransactionsAdapterMessage
{
    [JsonProperty("channel")]
    public string? Channel { get; set; }
    [JsonProperty("eventName")]
    public string? EventName { get; set; }
    [JsonProperty("payload")]
    public string? Payload { get; set; }
    [JsonProperty("requireRiskScore")]
    public bool? RequireRiskScore { get; set; }
    [JsonProperty("sourceLob")]
    public string? SourceLob { get; set; }
    [JsonProperty("transactionId")]
    public string? TransactionId { get; set; }
}
