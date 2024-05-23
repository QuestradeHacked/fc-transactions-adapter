using Domain.Models.Messages.Publisher;
using Domain.Services;
using Infra.Services.Publisher;
using Infra.Utils;
using Integration.Fixture;
using Integration.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Questrade.Library.PubSubClientHelper.Primitives;
using Xunit;

namespace Integration.Infra.Publisher;

public class TransactionAdapterPublisherTests(PubSubEmulatorProcessFixture pubSubFixture)
    : IClassFixture<PubSubEmulatorProcessFixture>
{
    private readonly IJsonSerializerOptionsProvider _defaultJsonSerializerOptionsProvider = new PubSubJsonSerializerOptions<TransactionAdapterResultMessage>();
    private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();
    private readonly IMemoryCache _memoryCache = Substitute.For<IMemoryCache>();
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
    private readonly IMetricService _metricService = Substitute.For<IMetricService>();
    private ILogger<TransactionAdapterPublisher> _logger;


    [Theory]
    [MemberData(nameof(TransactionsAdapterResultMessage))]
    public async Task TransactionAdapterPublisher_Should_PublishTheMessage(
        TransactionAdapterResultMessage transactionAdapterResultMessage)
    {
        // Arrange
        var topicId = $"T_{Guid.NewGuid()}";
        var subscriptionId = $"{topicId}.{Guid.NewGuid()}";

        pubSubFixture.CreateTopic(topicId);
        pubSubFixture.CreateSubscription(topicId, subscriptionId);

        var config =
            pubSubFixture.CreateDefaultPublisherConfig(topicId);

        _logger = Substitute.For<ILogger<TransactionAdapterPublisher>>();
        var pubSubPublisher = new TransactionAdapterPublisher(
            _loggerFactory,
            _memoryCache,
            _logger,
            (IJsonSerializerOptionsProvider<TransactionAdapterResultMessage>)_defaultJsonSerializerOptionsProvider,
            Options.Create(config).Value,
            Substitute.For<IHostApplicationLifetime>(),
            _metricService);

        // Act
        await pubSubPublisher.PublishAsync(transactionAdapterResultMessage);
        var receivedMessages =
            await pubSubFixture.PullMessagesAsync<TransactionAdapterResultMessage>(subscriptionId, true);

        // Assert
        Assert.Single(receivedMessages);
    }

    public static IEnumerable<object[]> TransactionsAdapterResultMessage()
    {
        yield return new object[]
        {
            new TransactionAdapterResultMessage
            {
                ReturnCode = "01",
                RiskAction = "Action",
                RiskLevel = "RiskLevel",
                RiskScore = "10",
                SessionId = Guid.NewGuid().ToString()
            }
        };
    }
}
