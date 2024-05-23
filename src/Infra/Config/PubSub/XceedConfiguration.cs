using Infra.Validators;

namespace Infra.Config.PubSub;

public class XceedConfiguration
{
    public string? BaseUrl { get; set; }
    public string? PlatformId { get; set; }
    public int Retry { get; set; }
    public string? SchemaVersion { get; set; }
    public string? SecretKey { get; set; }
    public string? ServiceId { get; set; }
    public string? TenantId { get; set; }
    public int Timeout { get; set; }

    public void Validate()
    {
        var validator = new XceedConfigurationValidator();

        var validationResult = validator.Validate(this);

        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(validationResult.Errors[0].ErrorMessage);
        }
    }
}
