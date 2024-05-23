using Domain.Models.Messages;
using Infra.Config.PubSub;
using Infra.Models.PubSub;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Questrade.Library.PubSubClientHelper.Primitives;
using Questrade.Library.PubSubClientHelper.Subscriber;

namespace Infra.Services.Subscriber;

public class TransactionsAdapterPubSubSubscriberBackgroundService(
    TransactionsAdapterSubscriberConfiguration subscriberConfiguration,
    ILoggerFactory loggerFactory,
    IMemoryCache cache,
    IJsonSerializerOptionsProvider<PubSubMessage<TransactionsAdapterMessage>> jsonProvider,
    ISubscriberMessageHandler<PubSubMessage<TransactionsAdapterMessage>> transactionSubscriberMessageHandlerHandler)
    : PubsubSubscriberBackgroundService<
        TransactionsAdapterSubscriberConfiguration, PubSubMessage<TransactionsAdapterMessage>>(loggerFactory, subscriberConfiguration,
        cache, jsonProvider, transactionSubscriberMessageHandlerHandler);
