using Domain.Models.Messages.Publisher;

namespace Domain.Services;

public interface ITransactionAdapterPublisher
{
    Task PublishAsync(TransactionAdapterResultMessage message, CancellationToken cancellationToken = default);
}
