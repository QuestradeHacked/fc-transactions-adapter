using Domain.Models.Messages;
using FluentValidation;
using Newtonsoft.Json.Linq;

namespace Infra.Validators;

public class TransactionsAdapterMessageValidator : AbstractValidator<TransactionsAdapterMessage>
{
    public TransactionsAdapterMessageValidator()
    {
        RuleFor(message => message.Channel).NotNull().NotEmpty().WithMessage("Channel must be valid");
        RuleFor(message => message.EventName).NotNull().NotEmpty().WithMessage("EventName must be valid");
        RuleFor(message => message.RequireRiskScore).NotNull().NotEmpty().WithMessage("RequireRiskScore must be valid");
        RuleFor(message => message.SourceLob).NotNull().NotEmpty().WithMessage("SourceLob must be valid");
        RuleFor(message => message.TransactionId).NotNull().NotEmpty().WithMessage("TransactionId must be valid");
        RuleFor(message => message.Payload)
            .Must(BeValidJson)
            .WithMessage("Payload must be a valid JSON string.");
    }

    private static bool BeValidJson(string? payload)
    {
        if (string.IsNullOrEmpty(payload))
        {
            return false;
        }

        try
        {
            JToken.Parse(payload);
            return true;
        }
        catch
        {
            return false;
        }
    }

}
