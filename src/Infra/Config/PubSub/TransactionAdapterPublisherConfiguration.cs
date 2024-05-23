using Domain.Models.Messages.Publisher;
using Microsoft.Extensions.Logging;
using Questrade.Library.PubSubClientHelper.Primitives;

namespace Infra.Config.PubSub;

public class TransactionAdapterPublisherConfiguration : BasePublisherConfiguration<TransactionAdapterResultMessage>
{
    public override Task HandleMessageLogAsync(ILogger logger, LogLevel logLevel, TransactionAdapterResultMessage message,
        string logMessage, CancellationToken cancellationToken = new CancellationToken())
    {
        logger.Log(logLevel, "{LogMessage} - Message contents: {@Message}", logMessage, message);

        return Task.CompletedTask;
    }

    public void Validate()
    {
        if (!Enable) return;

        if (string.IsNullOrWhiteSpace(ProjectId))
        {
            throw new InvalidOperationException("The configuration options for the TransactionsAdapterPublisher is not valid");
        }

        if (UseEmulator && string.IsNullOrWhiteSpace(Endpoint))
        {
            throw new InvalidOperationException("The emulator configuration options for TransactionsAdapterPublisher is not valid");
        }
    }
}
