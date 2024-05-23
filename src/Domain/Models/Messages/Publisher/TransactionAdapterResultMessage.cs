namespace Domain.Models.Messages.Publisher;

public class TransactionAdapterResultMessage
{
    public IEnumerable<RiskFactor>? RiskFactors { get; set; }
    public string? ReturnCode { get; set; }
    public string? RiskAction { get; set; }
    public string? RiskLevel { get; set; }
    public string? RiskScore { get; set; }
    public SearchMatch? SearchMatches { get; set; }
    public string? SessionId { get; set; }
}
