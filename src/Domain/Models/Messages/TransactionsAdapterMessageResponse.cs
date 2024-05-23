namespace Domain.Models.Messages;

public class TransactionsAdapterMessageResponse
{
    public string? EventId { get; set; }
    public string? FailReason { get; set; }
    public string? ReturnCode { get; set; }
    public string? RiskAction { get; set; }
    public List<RiskFactor> RiskFactors { get; set; } = [];
    public string? RiskLevel { get; set; }
    public decimal? RiskScore { get; set; }
    public List<SearchMatch> SearchMatches { get; set; } = [];
    public string? SessionId { get; set; }
    public string? TransactionId { get; set; }
}
