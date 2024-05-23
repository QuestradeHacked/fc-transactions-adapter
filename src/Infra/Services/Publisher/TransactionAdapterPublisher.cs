using Domain.Constants;
using Domain.Models.Messages.Publisher;
using Domain.Services;
using Infra.Config.PubSub;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Questrade.Library.PubSubClientHelper.Primitives;
using Questrade.Library.PubSubClientHelper.Publisher.Simple;

namespace Infra.Services.Publisher;

public sealed class TransactionAdapterPublisher :
    SimplePubsubPublisherService<TransactionAdapterPublisherConfiguration, TransactionAdapterResultMessage>,
    ITransactionAdapterPublisher
{
    private readonly ILogger<TransactionAdapterPublisher> _logger;
    private readonly IMetricService _metricService;
    public TransactionAdapterPublisher(
        ILoggerFactory loggerFactory,
        IMemoryCache memoryCache,
        ILogger<TransactionAdapterPublisher> logger,
        IJsonSerializerOptionsProvider<TransactionAdapterResultMessage> defaultJsonSerializerOptionsProvider,
        TransactionAdapterPublisherConfiguration publisherConfiguration,
        IHostApplicationLifetime hostApplicationLifetime,
        IMetricService metricService)
        : base(
            loggerFactory,
            memoryCache,
            publisherConfiguration,
            hostApplicationLifetime,
            defaultJsonSerializerOptionsProvider)
    {
        _logger = logger;
        _metricService = metricService;

        InitializeInnerClientAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task PublishAsync(TransactionAdapterResultMessage message, CancellationToken cancellationToken = default)
    {
        using var scope = _logDefineScope(_logger, nameof(TransactionAdapterPublisher), nameof(PublishAsync));

        try
        {
            _metricService.Increment(
                MetricNames.ResultPublisherRequestCount,
                new List<string>{MetricTags.StatusSuccess}
                );

            await PublishMessageAsync(message);

            _metricService.Increment
            (
                MetricNames.ResultPublisherRequestHandle,
                new List<string> { MetricTags.StatusSuccess }
            );

            _logMessagePublishedInformation(_logger, message.SessionId, null);
        }
        catch(Exception exception)
        {
            _metricService.Increment
            (
                MetricNames.ResultPublisherRequestHandle,
                new List<string>{ MetricTags.StatusPermanentError }
            );

            _logPublishingError(_logger, message.SessionId, exception);
        }
    }

    private static IDisposable _logDefineScope(ILogger logger, string? arg1, string? arg2)
    {
        ArgumentNullException.ThrowIfNull(logger);

        return LoggerMessage.DefineScope<string?, string?>(
                   "{TransactionsAdapterPublisherName}:{PublishAsyncName}")(logger, arg1, arg2) ??
               throw new InvalidOperationException("LoggerMessage.DefineScope returned null.");
    }

    private readonly Action<ILogger, string?, Exception?> _logMessagePublishedInformation =
        LoggerMessage.Define<string?>(
            eventId: new EventId(1, nameof(TransactionAdapterPublisher)),
            formatString: "Message Published: {SessionId}",
            logLevel: LogLevel.Information
        );

    private readonly Action<ILogger, string?, Exception?> _logPublishingError =
        LoggerMessage.Define<string?>(
            eventId: new EventId(2, nameof(TransactionAdapterPublisher)),
            formatString: "Error occurred when publishing the message: {SessionId}",
            logLevel: LogLevel.Error
        );
}
