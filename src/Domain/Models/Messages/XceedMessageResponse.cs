namespace Domain.Models.Messages;

public class XceedMessageResponse
{
    public string? ReturnCode { get; set; }
    public string? RiskAction { get; set; }
    public string? RiskLevel { get; set; }
    public string? RiskScore { get; set; }
    public string? SessionId { get; set; }

    public List<RiskFactor>? RiskFactors { get; set; }

    public SearchMatch? SearchMatches { get; set; }

    public bool IsReturnCodeSuccess()
    {
        return ReturnCode == "SUCCESS";
    }
}
