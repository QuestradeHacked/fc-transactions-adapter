using Domain.Models.Messages;
using FluentAssertions;
using Infra.Models.PubSub;
using Infra.Validators;

namespace Unit.Infra.Validators;

public class TransactionsAdapterMessageValidatorValidatorTests
{
    private readonly TransactionsAdapterMessageValidator _validator = new();

    [Theory]
    [MemberData(nameof(GetPossibleInvalidMessages))]
    public async Task ValidateAsync_WhenReceivesMessageWithoutChannelOrEventNameOrRequireRiskScoreOrOrSourceLobOrTransactionId_ShouldConsiderItAsInvalid(PubSubMessage<TransactionsAdapterMessage> transactionsAdapterMessage)
    {
        // Act
        var validationResult = await _validator.ValidateAsync(transactionsAdapterMessage.Data!);

        // Assert
        validationResult.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_WhenPayloadHasAnEmptyOrInvalidJson_ShouldConsiderItAsInvalid()
    {
        //Arrange
        var faker = new Bogus.Faker();

        var invalidJson = @"
            {
                name: 'John Doe',
                age: 30,
                address: {
                    city: 'New York',
                    state: 'NY'
                }
                email: 'john.doe@example.com'
            }";

        var message = new TransactionsAdapterMessage
        {
            Channel = faker.Random.String(),
            EventName = faker.Random.String(),
            Payload = invalidJson,
            RequireRiskScore = faker.Random.Bool(),
            SourceLob = faker.Random.String(),
            TransactionId = faker.Random.String()
        };

        // Act
        var validationResult = await _validator.ValidateAsync(message);

        // Assert
        validationResult.IsValid.Should().BeFalse();
    }

    public static IEnumerable<object[]> GetPossibleInvalidMessages()
    {
        var faker = new Bogus.Faker();

        yield return
        [
            new PubSubMessage<TransactionsAdapterMessage>
            {
                Id = faker.Random.Uuid().ToString(),
                Data = new TransactionsAdapterMessage
                {
                    Channel = faker.Random.String(),
                    EventName = null,
                    RequireRiskScore = null,
                    SourceLob = null,
                    TransactionId = null
                }
            }
        ];

        yield return
        [
            new PubSubMessage<TransactionsAdapterMessage>
            {
                Id = faker.Random.Uuid().ToString(),
                Data = new TransactionsAdapterMessage
                {
                    Channel = null,
                    EventName = faker.Random.String(),
                    RequireRiskScore = null,
                    SourceLob = null,
                    TransactionId = null
                }
            }
        ];

        yield return
        [
            new PubSubMessage<TransactionsAdapterMessage>
            {
                Id = faker.Random.Uuid().ToString(),
                Data = new TransactionsAdapterMessage
                {
                    Channel = null,
                    EventName = null,
                    RequireRiskScore = faker.Random.Bool(),
                    SourceLob = null,
                    TransactionId = null
                }
            }
        ];

        yield return
        [
            new PubSubMessage<TransactionsAdapterMessage>
            {
                Id = faker.Random.Uuid().ToString(),
                Data = new TransactionsAdapterMessage
                {
                    Channel = null,
                    EventName = null,
                    RequireRiskScore = null,
                    SourceLob = faker.Random.String(),
                    TransactionId = null
                }
            }
        ];

        yield return
        [
            new PubSubMessage<TransactionsAdapterMessage>
            {
                Id = faker.Random.Uuid().ToString(),
                Data = new TransactionsAdapterMessage
                {
                    Channel = null,
                    EventName = null,
                    RequireRiskScore = null,
                    SourceLob = null,
                    TransactionId = faker.Random.String()
                }
            }
        ];
    }
}
