using System.Globalization;
using Domain.Models.Messages;
using Domain.Services;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Infra.Models.PubSub;
using Infra.Services.Subscriber;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using Questrade.Library.PubSubClientHelper.Primitives;
using Unit.Fixture;

namespace Unit.Infra.Services.Subscriber;

public class TransactionsAdapterSubscriberMessageHandlerTests
{
    private readonly Bogus.Faker _faker = new();
    private readonly MockLogger<TransactionsAdapterSubscriberMessageHandler> _logger = new();
    private readonly Mock<IMetricService> _metricService = new();
    private readonly MessageOptions _options = new();
    private readonly TransactionsAdapterSubscriberMessageHandler _subscriberMessageHandler;
    private readonly TransactionsAdapterMessage _transactionMessage;
    private readonly Mock<IValidator<TransactionsAdapterMessage>> _validator = new();
    private readonly Mock<IXceedService> _xceedService = new();
    private readonly ITransactionAdapterPublisher _transactionAdapterPublisher;

    public TransactionsAdapterSubscriberMessageHandlerTests()
    {
        Mock<ILoggerFactory> loggerFactory = new();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_logger);

        _transactionAdapterPublisher = Substitute.For<ITransactionAdapterPublisher>();
        _subscriberMessageHandler = new TransactionsAdapterSubscriberMessageHandler(
            _logger,
            _metricService.Object,
            _validator.Object,
            _xceedService.Object,
            _transactionAdapterPublisher);

        _transactionMessage = new TransactionsAdapterMessage
        {
            Channel = _faker.Name.FirstName(),
            EventName = _faker.Name.FirstName(),
            Payload = "{ }",
            RequireRiskScore = _faker.Random.Bool(),
            SourceLob = _faker.Name.FirstName(),
            TransactionId = _faker.Random.Guid().ToString()
        };

        _validator
            .Setup(v => v.ValidateAsync(It.IsAny<TransactionsAdapterMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });
    }

    [Fact]
    public async Task HandleMessageAsync_ShouldLogError_WhenXceedThrowsException()
    {
        // Arrange
        _xceedService
            .Setup(x => x.SendMessage(It.IsAny<TransactionsAdapterMessage>()))
            .Throws(() => new Exception("Fail Test"));

        // Act
        var result =
            await _subscriberMessageHandler.HandleMessageAsync(
                new PubSubMessage<TransactionsAdapterMessage> { Data = _transactionMessage }, _options);

        // Assert
        result.Should().BeFalse();

        _logger.GetAllMessages().Should().Contain("HandleMessageAsync: Fail Test");
        _logger.GetAllMessages().Should().NotContain("All messages processed");

        _metricService.Verify(m =>
                m.Increment(It.IsAny<string>(), It.IsAny<IList<string>>()),
            Times.Exactly(2));
    }


    [Fact]
    public async Task HandleMessageAsync_ShouldProcessMessage_WhenAllInformationAreValid()
    {
        // Arrange
        _xceedService
            .Setup(x => x.SendMessage(It.IsAny<TransactionsAdapterMessage>()))
            .ReturnsAsync(new XceedMessageResponse
            {
                ReturnCode = "SUCCESS",
                RiskScore = _faker.Random.Decimal().ToString(CultureInfo.InvariantCulture),
                RiskAction = _faker.Random.Word(),
                RiskLevel = _faker.Random.Word(),
                SessionId = _faker.Random.Guid().ToString()
            });

        // Act
        var result =
            await _subscriberMessageHandler.HandleMessageAsync(
                new PubSubMessage<TransactionsAdapterMessage> { Data = _transactionMessage }, _options);

        // Assert
        result.Should().BeTrue();

        _logger.GetAllMessages().Should().Contain("All messages processed");

        _metricService.Verify(m =>
                m.Increment(It.IsAny<string>(), It.IsAny<IList<string>>()),
            Times.Exactly(2));
    }
}
