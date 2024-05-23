using System.Security.Cryptography;
using System.Text;
using Domain.Constants;
using Domain.Models;
using Domain.Models.Messages;
using Domain.Services;
using Infra.Config.PubSub;
using Infra.Utils;
using Microsoft.Extensions.Logging;
using SerilogTimings;

namespace Infra.Services.Xceed;

public class XceedService : IXceedService
{
    private readonly XceedConfiguration _configuration;
    private readonly ILogger<XceedService> _logger;
    private readonly IMetricService _metricService;
    private readonly IXceedRest _rest;

    public XceedService(
        XceedConfiguration configuration,
        IXceedRest rest,
        ILogger<XceedService> logger,
        IMetricService metricService)
    {
        _configuration = configuration;
        _logger = logger;
        _metricService = metricService;
        _rest = rest;
    }

    public async Task<XceedMessageResponse> SendMessage(TransactionsAdapterMessage message)
    {
        var payload = CreatePayload(message);

        var key = CreateKey(message, payload);

        var xceedParam = new XceedParams
        {
            RequireRiskscore = message.RequireRiskScore,
            PlatformId = _configuration.PlatformId,
            Channel = message.Channel,
            ServiceId = _configuration.ServiceId,
            Key = key
        };

        var timing = Operation.Begin(
            "Timing for Xceed request - {Class}.{Method}",
            nameof(XceedService), nameof(SendMessage));

        try
        {
            var xceedResponse = await _rest.SendAddEvent(xceedParam, payload);

            _metricService.Distribution(
                MetricNames.XceedApiHandleRequestCount,
                timing.Elapsed.TotalMilliseconds,
                [MetricTags.StatusOK, XceedMetricTags.AddEventEndpoint]
            );

            if (xceedResponse.IsReturnCodeSuccess())
            {
                _metricService.Distribution(
                    MetricNames.XceedApiHandleRequestCount,
                    timing.Elapsed.TotalMilliseconds,
                    [MetricTags.StatusOK, XceedMetricTags.AddEventEndpoint]);

                _logXceedSuccess(_logger, null);
            }
            else
            {
                _metricService.Distribution(
                    MetricNames.XceedApiHandleRequestCount,
                    timing.Elapsed.TotalMilliseconds,
                    [MetricTags.StatusNoOk, XceedMetricTags.AddEventEndpoint]);

                _logXceedFail(_logger, null);
            }

            return xceedResponse;
        }
        catch (Exception)
        {
            _metricService.Distribution(
                MetricNames.XceedApiHandleRequestCount,
                timing.Elapsed.TotalMilliseconds,
                new List<string>
                {
                    MetricTags.StatusPermanentError,
                    XceedMetricTags.AddEventEndpoint
                }
            );
            throw;
        }
    }

    private string CreatePayload(TransactionsAdapterMessage message)
    {
        var addEvent = CreateEvent(message);

        var jsonEvent = JsonUtils.Serialize(addEvent);

        var jsonMerged = JsonUtils.MergeJson(message.Payload!, jsonEvent);

        return jsonMerged ?? string.Empty;
    }

    private string CreateKey(TransactionsAdapterMessage message, string body)
    {
        var keyStr = _configuration.PlatformId +
                     _configuration.ServiceId +
                     _configuration.SecretKey +
                     message.Channel +
                     body;

        var keyBytes = Encoding.UTF8.GetBytes(keyStr);

        var computeHash = SHA256.HashData(keyBytes);

        var keyHash = string.Concat(computeHash.Select(b => b.ToString("x2")));

        return keyHash;
    }

    private XceedEvent CreateEvent(TransactionsAdapterMessage message)
    {
        return new XceedEvent
        {
            GAEvent = new GaEvent
            {
                ApiEventName = message.EventName,
                Common = new XceedCommon
                {
                    Channel = message.Channel,
                    ProviderEventName = null,
                    SchemaVersion = _configuration.SchemaVersion,
                    TenantId = _configuration.TenantId
                }
            }
        };
    }

    private readonly Action<ILogger, Exception?> _logXceedFail =
        LoggerMessage.Define(
            eventId: new EventId(1, nameof(XceedService)),
            formatString: "Message processed with error in Xceed",
            logLevel: LogLevel.Information
        );

    private readonly Action<ILogger, Exception?> _logXceedSuccess =
        LoggerMessage.Define(
            eventId: new EventId(2, nameof(XceedService)),
            formatString: "Message processed with success in Xceed",
            logLevel: LogLevel.Information
        );
}
