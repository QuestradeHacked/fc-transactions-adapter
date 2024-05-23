using Domain.Constants;
using Domain.Models.Messages;
using Domain.Models.Messages.Publisher;
using Domain.Services;
using FluentValidation;
using FluentValidation.Results;
using Infra.Models.PubSub;
using Microsoft.Extensions.Logging;
using Questrade.Library.PubSubClientHelper.Primitives;
using Questrade.Library.PubSubClientHelper.Subscriber;

namespace Infra.Services.Subscriber;

public class
    TransactionsAdapterSubscriberMessageHandler : ISubscriberMessageHandler<PubSubMessage<TransactionsAdapterMessage>>
{
    private readonly ILogger<TransactionsAdapterSubscriberMessageHandler> _logger;
    private readonly IMetricService _metricService;
    private readonly IValidator<TransactionsAdapterMessage> _validator;
    private readonly IXceedService _xceedService;
    private readonly ITransactionAdapterPublisher _transactionAdapterPublisher;

    public TransactionsAdapterSubscriberMessageHandler(
        ILogger<TransactionsAdapterSubscriberMessageHandler> logger,
        IMetricService metricService,
        IValidator<TransactionsAdapterMessage> validator,
        IXceedService xceedService,
        ITransactionAdapterPublisher transactionAdapterPublisher)
    {
        _logger = logger;
        _metricService = metricService;
        _validator = validator;
        _xceedService = xceedService;
        _transactionAdapterPublisher = transactionAdapterPublisher;
    }

    public async Task<bool> HandleMessageAsync(PubSubMessage<TransactionsAdapterMessage> message,
        MessageOptions messageOptions,
        CancellationToken cancellationToken = new())
    {
        using var scope = _logDefineScope(_logger, nameof(TransactionsAdapterSubscriberMessageHandler),
            nameof(HandleMessageAsync));

        _logMessageReceivedInformation(_logger, message.Data!, null);

        _metricService.Increment(MetricNames.TransactionsAdapterSubscriberMessageCount, [MetricTags.StatusSuccess]);

        var validationResult = await _validator.ValidateAsync(message.Data!, cancellationToken);

        if (!validationResult.IsValid)
        {
            _logMissingRequiredInformationWarning(_logger, validationResult.Errors, null);

            _metricService.Increment(MetricNames.TransactionsAdapterSubscriberMessageHandle,[MetricTags.StatusSuccess]);

            return true;
        }

        try
        {
            var result = await _xceedService.SendMessage(message.Data!);

            await _transactionAdapterPublisher.PublishAsync(new TransactionAdapterResultMessage{
                RiskAction = result.RiskAction,
                RiskScore = result.RiskScore,
                RiskLevel = result.RiskLevel,
                ReturnCode = result.ReturnCode,
                SearchMatches = result.SearchMatches == null ? null : new Domain.Models.Messages.Publisher.SearchMatch{Name = result.SearchMatches.Name},
                SessionId = result.SessionId,
                RiskFactors = result.RiskFactors?.Select(rf => new Domain.Models.Messages.Publisher.RiskFactor{Name = rf.Name})
            }, cancellationToken);

            _logMessagesProcessedWithSuccess(_logger, null);

            _metricService.Increment(MetricNames.TransactionsAdapterSubscriberMessageHandle, [MetricTags.StatusSuccess]);

            return true;
        }
        catch (Exception e)
        {
            _logXceedError(_logger, nameof(HandleMessageAsync), e.Message, e);

            _metricService.Increment(MetricNames.TransactionsAdapterSubscriberMessageHandle,[MetricTags.StatusPermanentError]);

            return false;
        }
    }



    private static IDisposable _logDefineScope(ILogger logger, string? arg1, string? arg2)
    {
        ArgumentNullException.ThrowIfNull(logger);

        return LoggerMessage.DefineScope<string?, string?>(
                   "{TransactionsAdapterSubscriberName}:{HandleReceivedMessageAsyncName}")(logger, arg1, arg2) ??
               throw new InvalidOperationException("LoggerMessage.DefineScope returned null.");
    }

    private readonly Action<ILogger, TransactionsAdapterMessage, Exception?> _logMessageReceivedInformation =
        LoggerMessage.Define<TransactionsAdapterMessage>(
            eventId: new EventId(1, nameof(TransactionsAdapterMessage)),
            formatString: "Message processed: {TransactionsAdapterMessage}",
            logLevel: LogLevel.Information
        );

    private readonly Action<ILogger, List<ValidationFailure>?, Exception?> _logMissingRequiredInformationWarning =
        LoggerMessage.Define<List<ValidationFailure>?>(
            eventId: new EventId(2, nameof(TransactionsAdapterPubSubSubscriberBackgroundService)),
            formatString: "Invalid message: {@ValidationFailures}.",
            logLevel: LogLevel.Warning
        );

    private readonly Action<ILogger, string?, string?, Exception?>
        _logXceedError =
            LoggerMessage.Define<string?, string?>(
                eventId: new EventId(3, nameof(TransactionsAdapterPubSubSubscriberBackgroundService)),
                formatString: "{MethodName}: {ErrorMessage}",
                logLevel: LogLevel.Error
            );

    private readonly Action<ILogger, Exception?> _logMessagesProcessedWithSuccess =
        LoggerMessage.Define(
            eventId: new EventId(4, nameof(TransactionsAdapterPubSubSubscriberBackgroundService)),
            formatString: "All messages processed",
            logLevel: LogLevel.Information
        );
}
