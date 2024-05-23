using Domain.Models.Messages;
using Infra.Models.PubSub;
using Microsoft.Extensions.Logging;
using Questrade.Library.PubSubClientHelper.Primitives;

namespace Infra.Config.PubSub;

public class TransactionsAdapterSubscriberConfiguration : BaseSubscriberConfiguration<PubSubMessage<TransactionsAdapterMessage>>
{
    public override Task HandleMessageLogAsync(ILogger logger, LogLevel logLevel, PubSubMessage<TransactionsAdapterMessage> message, string logMessage, Exception? error = null, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public void Validate()
    {
        if (!Enable) return;

        if (string.IsNullOrWhiteSpace(ProjectId) || string.IsNullOrWhiteSpace(SubscriptionId))
        {
            throw new InvalidOperationException("The configuration options for the TransactionsAdapterSubscriber is not valid");
        }

        if (UseEmulator && string.IsNullOrWhiteSpace(Endpoint))
        {
            throw new InvalidOperationException("The emulator configuration options for TransactionsAdapterSubscriber is not valid");
        }
    }
}
