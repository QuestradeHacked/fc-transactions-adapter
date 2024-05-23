using Refit;

namespace Domain.Models;

public class XceedParams
{
    [AliasAs("channel")]
    public string? Channel { get; set; }

    [AliasAs("key")]
    public string? Key { get; set; }

    [AliasAs("platformId")]
    public string? PlatformId { get; set; }

    [AliasAs("requireRiskscore")]
    public bool? RequireRiskscore { get; set; }

    [AliasAs("serviceId")]
    public string? ServiceId { get; set; }
}
