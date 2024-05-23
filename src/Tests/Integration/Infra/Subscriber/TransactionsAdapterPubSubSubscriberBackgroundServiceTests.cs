using Domain.Models;
using Domain.Models.Messages;
using Domain.Services;
using FluentValidation;
using FluentValidation.Results;
using Infra.Config.PubSub;
using Infra.Models.PubSub;
using Infra.Services.Subscriber;
using Infra.Services.Xceed;
using Integration.Fixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NSubstitute;
using Questrade.Library.PubSubClientHelper.Primitives;
using Xunit;

namespace Integration.Infra.Subscriber;

public class TransactionsAdapterPubSubSubscriberBackgroundServiceTests : IAssemblyFixture<PubSubEmulatorProcessFixture>
{
    private readonly TransactionsAdapterPubSubSubscriberBackgroundService
        _transactionsAdapterPubSubSubscriberBackgroundService;

    private readonly IMetricService _metricService;

    private readonly MockLogger<TransactionsAdapterSubscriberMessageHandler> _logger;

    private readonly PubSubEmulatorProcessFixture _pubSubFixture;

    private readonly IValidator<TransactionsAdapterMessage> _validator;

    private readonly int _subscriberTimeout;

    private readonly string _topicId;

    private readonly Bogus.Faker _faker = new();
    private ITransactionAdapterPublisher _transactionAdapterPublisher;

    public TransactionsAdapterPubSubSubscriberBackgroundServiceTests(PubSubEmulatorProcessFixture pubSubFixture)
    {
        _logger = new MockLogger<TransactionsAdapterSubscriberMessageHandler>();

        Mock<ILoggerFactory> loggerFactory = new();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_logger);

        _metricService = Substitute.For<IMetricService>();
        _pubSubFixture = pubSubFixture;
        _subscriberTimeout = _pubSubFixture.SubscriberTimeout;
        _topicId = $"T_{Guid.NewGuid()}";

        _validator = Substitute.For<IValidator<TransactionsAdapterMessage>>();

        var subscriptionId = $"{_topicId}.{Guid.NewGuid()}";
        var subscriberConfig = _pubSubFixture.CreateDefaultSubscriberConfig(subscriptionId);

        var memoryCache = new Mock<IMemoryCache>();

        var jsonSerializeProvider = new JsonSerializerOptionsProvider<PubSubMessage<TransactionsAdapterMessage>>();

        var subscriberMessageHandlerHandler = CreateSubscriberMessageHandler();

        _transactionsAdapterPubSubSubscriberBackgroundService =
            new TransactionsAdapterPubSubSubscriberBackgroundService(
                subscriberConfig,
                loggerFactory.Object,
                memoryCache.Object,
                jsonSerializeProvider,
                subscriberMessageHandlerHandler
            );

        _pubSubFixture.CreateTopic(_topicId);
        _pubSubFixture.CreateSubscription(_topicId, subscriptionId);
    }

    [Fact]
    public async Task HandleReceivedMessageAsync_ShouldLogInformation_WhenWorkflowIsDone()
    {
        // Arrange
        _validator.ValidateAsync(Arg.Any<TransactionsAdapterMessage>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(new ValidationResult());

        var publisher = await _pubSubFixture.CreatePublisherAsync(_topicId);

        var pubSubMessage = new PubSubMessage<TransactionsAdapterMessage>
        {
            Id = _faker.Random.Uuid().ToString(),
            Time = DateTime.Now,
            Data = new TransactionsAdapterMessage
            {
                Channel = _faker.Random.String(),
                EventName = _faker.Random.String(),
                RequireRiskScore = _faker.Random.Bool(),
                SourceLob = _faker.Random.String(),
                TransactionId = _faker.Random.String()
            }
        };

        var jsonPubSubMessage = JsonConvert.SerializeObject(pubSubMessage);

        // Act
        await publisher.PublishAsync(jsonPubSubMessage);
        await _transactionsAdapterPubSubSubscriberBackgroundService.StartAsync(CancellationToken.None);
        await Task.Delay(_subscriberTimeout, CancellationToken.None);
        await _transactionsAdapterPubSubSubscriberBackgroundService.StopAsync(CancellationToken.None);

        var loggedMessages = _logger.GetAllMessages();

        // Assert
        Assert.Contains("All messages processed", loggedMessages);
        _metricService.Received(2).Increment(
            metricName: Arg.Any<string>(),
            metricTags: Arg.Any<List<string>>()
        );
    }

    [Fact]
    public async Task HandleReceivedMessageAsync_ShouldLogWarning_WhenValidationFailed()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var publisher = await _pubSubFixture.CreatePublisherAsync(_topicId);

        var eventNameErrorMessage = "EventName must be Valid";
        var eventNameValidationFailure = new ValidationFailure("eventName", eventNameErrorMessage);

        _validator
            .ValidateAsync(Arg.Any<TransactionsAdapterMessage>(), cancellationToken)
            .ReturnsForAnyArgs(new ValidationResult
            {
                Errors = [eventNameValidationFailure]
            });

        var transactionsAdapterMessage = new PubSubMessage<TransactionsAdapterMessage>
        {
            Id = _faker.Random.Uuid().ToString(),
            Data = new TransactionsAdapterMessage
            {
                Channel = _faker.Random.String(),
                EventName = null,
                RequireRiskScore = _faker.Random.Bool(),
                SourceLob = _faker.Random.String(),
                TransactionId = _faker.Random.String()
            }
        };

        // Act
        await publisher.PublishAsync(JsonConvert.SerializeObject(transactionsAdapterMessage.Data));
        await _transactionsAdapterPubSubSubscriberBackgroundService.StartAsync(CancellationToken.None);
        await Task.Delay(_subscriberTimeout, CancellationToken.None);
        await _transactionsAdapterPubSubSubscriberBackgroundService.StopAsync(CancellationToken.None);

        var loggedMessages = _logger.GetAllMessages();

        // Assert
        Assert.Contains("Invalid message: EventName must be Valid.", loggedMessages);
        _metricService.Received(2).Increment(
            metricName: Arg.Any<string>(),
            metricTags: Arg.Any<List<string>>()
        );
    }

    private TransactionsAdapterSubscriberMessageHandler CreateSubscriberMessageHandler()
    {
        var logger = new MockLogger<XceedService>();

        var metricService = new Mock<IMetricService>();

        var xceedConfiguration = new XceedConfiguration
        {
            BaseUrl = _faker.Internet.Url(),
            PlatformId = _faker.Random.Guid().ToString(),
            Retry = 3,
            SchemaVersion = _faker.System.Version().ToString(),
            SecretKey = _faker.Random.Words(),
            ServiceId = _faker.Random.Guid().ToString(),
            TenantId = _faker.Random.Guid().ToString(),
            Timeout = 5000,
        };

        var xceedRest = new Mock<IXceedRest>();
        xceedRest
            .Setup(x => x.SendAddEvent(It.IsAny<XceedParams>(), It.IsAny<string>()))
            .ReturnsAsync(new XceedMessageResponse { ReturnCode = "SUCCESS" });

        var xceedService = new Mock<XceedService>(xceedConfiguration, xceedRest.Object, logger, metricService.Object);
        _transactionAdapterPublisher = Substitute.For<ITransactionAdapterPublisher>();
        return new TransactionsAdapterSubscriberMessageHandler(_logger, _metricService, _validator,
            xceedService.Object, _transactionAdapterPublisher);
    }
}
