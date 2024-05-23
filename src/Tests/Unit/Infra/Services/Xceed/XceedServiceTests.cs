using System.Reflection;
using Domain.Models;
using Domain.Models.Messages;
using Domain.Services;
using FluentAssertions;
using Infra.Config.PubSub;
using Infra.Services.Xceed;
using Microsoft.Extensions.Logging;
using Moq;
using Unit.Fixture;

namespace Unit.Infra.Services.Xceed;

public class XceedServiceTests
{
    private readonly XceedConfiguration _configuration;
    private readonly MockLogger<XceedService> _logger = new();
    private readonly TransactionsAdapterMessage _message;
    private readonly Mock<IXceedRest> _rest = new();
    private readonly IXceedService _service;


    public XceedServiceTests()
    {
        var metricService = new Mock<IMetricService>();

        Mock<ILoggerFactory> loggerFactory = new();
        loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_logger);

        _configuration = XceedFixture.CreateConfiguration();

        _message = XceedFixture.CreateMessage();

        _service = new XceedService(_configuration, _rest.Object, _logger, metricService.Object);
    }

    [Fact]
    public async Task SendMessage_ShouldSendMessage_WhenPayloadIsInformed()
    {
        // Arrange
        _rest
            .Setup(r => r.SendAddEvent(It.IsAny<XceedParams>(), It.IsAny<string>()))
            .ReturnsAsync(new XceedMessageResponse { ReturnCode = "SUCCESS" });


        // Act
        var response = await _service.SendMessage(_message);

        // Assert
        response.Should().NotBeNull();
        response.IsReturnCodeSuccess().Should().BeTrue();

        _rest.Verify(r => r.SendAddEvent(It.IsAny<XceedParams>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void CreateKey_ShouldCreateTheKey_WhenMessageAndBodyAreInformed()
    {
        // Arrange

        //Act
        var key = CreateKey(_message, string.Empty);

        // Assert
        key.Should().NotBeNullOrEmpty();
        key.Should().Be("9d47cf6fd077dc7bafdf4d7b59612a133df049dd05deee5f9d7a2ce4290e7c65");
    }

    [Fact]
    public void CreateEvent_ShouldConvertCorrectly_WhenCreateEventIsCalled()
    {
        // Arrange

        // Act
        var xceedEvent = CreateEvent(_message);

        // Assert
        xceedEvent!.GAEvent.ApiEventName.Should().Be(_message.EventName);
        xceedEvent.GAEvent.Common!.Channel.Should().Be(_message.Channel);
        xceedEvent.GAEvent.Common.ProviderEventName.Should().BeNull();
        xceedEvent.GAEvent.Common.SchemaVersion.Should().Be(_configuration.SchemaVersion);
        xceedEvent.GAEvent.Common.TenantId.Should().Be(_configuration.TenantId);
    }

    [Fact]
    public void CreateEvent_ShouldCreatePayload_WhenMethodIsCalled()
    {
        // Arrange

        // Act
        var payload = CreatePayload(_message);

        // Assert

        payload.Should().NotBeNullOrEmpty();
        payload!.Should().Contain("APIEventName");
        payload.Should().Contain("payload_test");
    }

    private string? CreatePayload(TransactionsAdapterMessage message)
    {
        var methodInfo = _service.GetType().GetMethod("CreatePayload", BindingFlags.NonPublic | BindingFlags.Instance);

        object[] parameters = [message];

        return methodInfo!.Invoke(_service, parameters) as string;
    }

    private string? CreateKey(TransactionsAdapterMessage message, string body)
    {
        var methodInfo = _service.GetType().GetMethod("CreateKey", BindingFlags.NonPublic | BindingFlags.Instance);

        object[] parameters = [message, body];

        return methodInfo!.Invoke(_service, parameters) as string;
    }

    private XceedEvent? CreateEvent(TransactionsAdapterMessage message)
    {
        var methodInfo = _service.GetType().GetMethod("CreateEvent", BindingFlags.NonPublic | BindingFlags.Instance);

        object[] parameters = [message];

        return methodInfo!.Invoke(_service, parameters) as XceedEvent;
    }
}
