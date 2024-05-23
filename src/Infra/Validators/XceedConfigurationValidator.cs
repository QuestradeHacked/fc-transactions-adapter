using FluentValidation;
using Infra.Config.PubSub;

namespace Infra.Validators;

public class XceedConfigurationValidator : AbstractValidator<XceedConfiguration>
{
    public XceedConfigurationValidator()
    {
        RuleFor(x => x.BaseUrl).NotNull().WithMessage("BaseUrl must be valid");
        RuleFor(x => x.PlatformId).NotNull().WithMessage("PlatformId must be valid");
        RuleFor(x => x.SchemaVersion).NotNull().WithMessage("SchemaVersion must be valid");
        RuleFor(x => x.SecretKey).NotNull().WithMessage("SecretKey must be valid");
        RuleFor(x => x.ServiceId).NotNull().WithMessage("ServiceId must be valid");
        RuleFor(x => x.TenantId).NotNull().WithMessage("TenantID must be valid");
    }
}
