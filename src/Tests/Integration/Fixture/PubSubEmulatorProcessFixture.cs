using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Infra.Config.PubSub;
using Integration.Config;
using Questrade.Library.PubSubClientHelper.Primitives;

namespace Integration.Fixture;

public class PubSubEmulatorProcessFixture
{
    private readonly AppSettings _appSettings = new();

    private readonly JsonSerializerOptions _serializeOptions;

    private readonly PublisherServiceApiClient _publisherServiceApiClient;

    private readonly SubscriberServiceApiClient _subscriberServiceApiClient;

    private string Endpoint { get; }

    private string ProjectId { get; }

    public int SubscriberTimeout { get; }

    public PubSubEmulatorProcessFixture()
    {
        Endpoint = $"{_appSettings.GetProcessPubSubHost()}:{_appSettings.GetProcessPubSubPort()}";
        Environment.SetEnvironmentVariable("PUBSUB_EMULATOR_HOST", Endpoint);

        ProjectId = _appSettings.GetPubSubProjectId();
        SubscriberTimeout = _appSettings.GetPubSubSubscriberTimeout();

        _publisherServiceApiClient = CreatePublisherServiceApiClient();
        _subscriberServiceApiClient = CreateSubscriberServiceApiClient();

        _serializeOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
    }

    public void CreateTopic(string topicId)
    {
        var topicName = TopicName.FromProjectTopic(ProjectId, topicId);
        var topic = _publisherServiceApiClient.CreateTopic(topicName);

        Console.WriteLine($"Topic {topic.Name} created.");
    }

    public void CreateSubscription(string topicId, string subscriptionId)
    {
        var topicName = TopicName.FromProjectTopic(ProjectId, topicId);
        var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, subscriptionId);

        _subscriberServiceApiClient.CreateSubscription(
            new Subscription
            {
                TopicAsTopicName = topicName,
                SubscriptionName = subscriptionName,
                EnableMessageOrdering = true
            }
        );

        Console.WriteLine($"Subscription {subscriptionId} created.");
    }

    public async Task<PublisherClient> CreatePublisherAsync(string topicId)
    {
        var publisherClientBuilder = new PublisherClientBuilder
        {
            ApiSettings = PublisherServiceApiSettings.GetDefault(),
            ChannelCredentials = ChannelCredentials.Insecure,
            Endpoint = Endpoint,
            Settings = new PublisherClient.Settings
            {
                EnableMessageOrdering = true
            },
            TopicName = TopicName.FromProjectTopic(ProjectId, topicId)
        };

        var publisher = await publisherClientBuilder.BuildAsync();

        return publisher;
    }

    public TransactionsAdapterSubscriberConfiguration CreateDefaultSubscriberConfig(string subscriptionId)
    {
        return new TransactionsAdapterSubscriberConfiguration
        {
            AcknowledgeDeadline = TimeSpan.FromSeconds(30),
            AcknowledgeExtensionWindow = TimeSpan.FromSeconds(10),
            Enable = true,
            Endpoint = Endpoint,
            MaximumOutstandingByteCount = 1,
            MaximumOutstandingElementCount = 1,
            ProjectId = ProjectId,
            SubscriberClientCount = 1,
            SubscriptionId = subscriptionId,
            UseEmulator = true,
            MessageMetadataDisposition = MessageMetadataDisposition.None
        };
    }

    public TransactionAdapterPublisherConfiguration CreateDefaultPublisherConfig(string topicId)
    {
        return new TransactionAdapterPublisherConfiguration
        {
            Enable = true,
            Endpoint = Endpoint,
            ProjectId = ProjectId,
            UseEmulator = true,
            MessageMetadataDisposition = MessageMetadataDisposition.None,
            TopicId = topicId
        };
    }

    private async Task<SubscriberClient> CreateSubscriberAsync(string subscriptionId)
    {
        var subscriptionName = SubscriptionName.FromProjectSubscription(ProjectId, subscriptionId);
        var clientBuilder = new SubscriberClientBuilder
        {
            Endpoint = Endpoint,
            ChannelCredentials = ChannelCredentials.Insecure,
            ApiSettings = SubscriberServiceApiSettings.GetDefault(),
            SubscriptionName = subscriptionName
        };

        return await clientBuilder.BuildAsync();
    }

    public async Task<List<TData>> PullMessagesAsync<TData>(string subscriptionId, bool acknowledge)
    {
        var subscriber = await CreateSubscriberAsync(subscriptionId);
        var receivedMessages = new List<TData>();
        var startTask = subscriber.StartAsync((message, _) =>
        {
            var dataJson = message.Data.ToStringUtf8();
            var data = JsonSerializer.Deserialize<TData>(dataJson, _serializeOptions);
            receivedMessages.Add(data!);
            return Task.FromResult(acknowledge ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack);
        });

        await Task.Delay(SubscriberTimeout);
        await subscriber.StopAsync(CancellationToken.None);
        await startTask;

        return receivedMessages;
    }

    private PublisherServiceApiClient CreatePublisherServiceApiClient()
    {
        var publisher = new PublisherServiceApiClientBuilder
        {
            ChannelCredentials = ChannelCredentials.Insecure,
            Endpoint = Endpoint
        }.Build();

        return publisher;
    }

    private SubscriberServiceApiClient CreateSubscriberServiceApiClient()
    {
        var subscriber = new SubscriberServiceApiClientBuilder
        {
            ChannelCredentials = ChannelCredentials.Insecure,
            Endpoint = Endpoint
        }.Build();

        return subscriber;
    }
}
