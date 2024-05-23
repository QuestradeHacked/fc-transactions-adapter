using Domain.Models.Messages;

namespace Domain.Services;

public interface IXceedService
{
    Task<XceedMessageResponse> SendMessage(TransactionsAdapterMessage message);
}
