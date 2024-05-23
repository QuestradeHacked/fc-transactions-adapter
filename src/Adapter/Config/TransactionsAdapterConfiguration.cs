using Domain.Models.Messages;
using Infra.Config.PubSub;
using Questrade.Library.PubSubClientHelper.Primitives;

namespace Adapter.Config;

public class TransactionsAdapterConfiguration : BaseSubscriberConfiguration<TransactionsAdapterMessage>
{
    public TransactionsAdapterSubscriberConfiguration TransactionsAdapterSubscriberConfiguration { get; } = new();

    public TransactionAdapterPublisherConfiguration TransactionAdapterPublisherConfiguration { get; } = new();

    public XceedConfiguration XceedConfiguration { get; } = new();

    public override Task HandleMessageLogAsync(ILogger logger, LogLevel logLevel, TransactionsAdapterMessage message, string logMessage, Exception? error = null, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    internal void Validate()
    {
        if (TransactionsAdapterSubscriberConfiguration == null)
        {
            throw new InvalidOperationException(message: "TransactionsAdapterSubscriberConfiguration is not valid.");
        }

        TransactionsAdapterSubscriberConfiguration.Validate();

        if (XceedConfiguration == null)
        {
            throw new InvalidOperationException(message: "XceedConfiguration is not valid.");
        }

        XceedConfiguration.Validate();

        if (TransactionAdapterPublisherConfiguration == null)
        {
            throw new InvalidOperationException(message: "TransactionAdapterPublisherConfiguration is not valid.");
        }

        TransactionAdapterPublisherConfiguration.Validate();

    }
}
