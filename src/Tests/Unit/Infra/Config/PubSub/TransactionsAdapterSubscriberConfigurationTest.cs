using Infra.Config.PubSub;

namespace Unit.Infra.Config.PubSub;

public class TransactionsAdapterSubscriberConfigurationTest
{
    private readonly Bogus.Faker _faker = new();

    [Fact]
    public void Validate_ShouldRaiseException_WhenProjectIdIsNull()
    {
        // Arrange
        TransactionsAdapterSubscriberConfiguration transactionsAdapterSubscriberConfiguration = new()
        {
            ProjectId = null
        };

        // Act
        var exception = Record.Exception(() => transactionsAdapterSubscriberConfiguration.Validate());

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("The configuration options for the TransactionsAdapterSubscriber is not valid", exception.Message);
    }

    [Fact]
    public void Validate_ShouldRaiseException_WhenProjectIdIsEmpty()
    {
        // Arrange
        TransactionsAdapterSubscriberConfiguration transactionsAdapterSubscriberConfiguration = new()
        {
            ProjectId = string.Empty
        };

        // Act
        var exception = Record.Exception(() => transactionsAdapterSubscriberConfiguration.Validate());

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("The configuration options for the TransactionsAdapterSubscriber is not valid", exception.Message);
    }

    [Fact]
    public void Validate_ShouldRaiseException_WhenProjectIdIsAWhiteSpace()
    {
        // Arrange
        TransactionsAdapterSubscriberConfiguration transactionsAdapterSubscriberConfiguration = new()
        {
            ProjectId = " "
        };

        // Act
        var exception = Record.Exception(() => transactionsAdapterSubscriberConfiguration.Validate());

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("The configuration options for the TransactionsAdapterSubscriber is not valid", exception.Message);
    }

    [Fact]
    public void Validate_ShouldRaiseException_WhenSubscriptionIdIsNull()
    {
        // Arrange
        TransactionsAdapterSubscriberConfiguration transactionsAdapterSubscriberConfiguration = new()
        {
            ProjectId = _faker.Internet.Random.ToString(),
            SubscriptionId = null
        };

        // Act
        var exception = Record.Exception(() => transactionsAdapterSubscriberConfiguration.Validate());

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("The configuration options for the TransactionsAdapterSubscriber is not valid", exception.Message);
    }

    [Fact]
    public void Validate_ShouldRaiseException_WhenSubscriptionIdIsEmpty()
    {
        // Arrange
        TransactionsAdapterSubscriberConfiguration transactionsAdapterSubscriberConfiguration = new()
        {
            ProjectId = _faker.Internet.Random.ToString(),
            SubscriptionId = string.Empty
        };

        // Act
        var exception = Record.Exception(() => transactionsAdapterSubscriberConfiguration.Validate());

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("The configuration options for the TransactionsAdapterSubscriber is not valid", exception.Message);
    }

    [Fact]
    public void Validate_ShouldRaiseException_WhenSubscriptionIdIsAWhiteSpace()
    {
        // Arrange
        TransactionsAdapterSubscriberConfiguration transactionsAdapterSubscriberConfiguration = new()
        {
            ProjectId = _faker.Internet.Random.ToString(),
            SubscriptionId = " "
        };

        // Act
        var exception = Record.Exception(() => transactionsAdapterSubscriberConfiguration.Validate());

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("The configuration options for the TransactionsAdapterSubscriber is not valid", exception.Message);
    }

    [Fact]
    public void Validate_ShouldRaiseException_WhenEmulatorIsTrue_AndEndpointIsEmpty()
    {
        // Arrange
        TransactionsAdapterSubscriberConfiguration transactionsAdapterSubscriberConfiguration = new()
        {
            ProjectId = _faker.Internet.Random.ToString(),
            SubscriptionId = _faker.Internet.Random.ToString(),
            UseEmulator = true,
            Endpoint = string.Empty
        };

        // Act
        var exception = Record.Exception(() => transactionsAdapterSubscriberConfiguration.Validate());

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("The emulator configuration options for TransactionsAdapterSubscriber is not valid",
            exception.Message);
    }

    [Fact]
    public void Validate_ShouldRaiseException_WhenEmulatorIsTrue_AndEndpointIsNull()
    {
        // Arrange
        TransactionsAdapterSubscriberConfiguration transactionsAdapterSubscriberConfiguration = new()
        {
            ProjectId = _faker.Internet.Random.ToString(),
            SubscriptionId = _faker.Internet.Random.ToString(),
            UseEmulator = true,
            Endpoint = null
        };

        // Act
        var exception = Record.Exception(() => transactionsAdapterSubscriberConfiguration.Validate());

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("The emulator configuration options for TransactionsAdapterSubscriber is not valid",
            exception.Message);
    }

    [Fact]
    public void Validate_ShouldRaiseException_WhenEmulatorIsTrue_AndEndpointHasWhiteSpace()
    {
        // Arrange
        TransactionsAdapterSubscriberConfiguration transactionsAdapterSubscriberConfiguration = new()
        {
            ProjectId = _faker.Internet.Random.ToString(),
            SubscriptionId = _faker.Internet.Random.ToString(),
            UseEmulator = true,
            Endpoint = " "
        };

        // Act
        var exception = Record.Exception(() => transactionsAdapterSubscriberConfiguration.Validate());

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("The emulator configuration options for TransactionsAdapterSubscriber is not valid",
            exception.Message);
    }

    [Fact]
    public void Validate_ShouldNotThrow_WhenAllConfigurationsAreCorrect()
    {
        // Arrange
        TransactionsAdapterSubscriberConfiguration transactionsAdapterSubscriberConfiguration = new()
        {
            Enable = false
        };

        // Act
        var exception = Record.Exception(() => transactionsAdapterSubscriberConfiguration.Validate());

        // Assert
        Assert.Null(exception);
    }
}
