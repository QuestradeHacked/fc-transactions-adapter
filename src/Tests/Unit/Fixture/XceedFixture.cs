using Domain.Models.Messages;
using Infra.Config.PubSub;

namespace Unit.Fixture;

public static class XceedFixture
{
    public static XceedConfiguration CreateConfiguration()
    {
        return new XceedConfiguration
        {
            BaseUrl = "http://localhost:3000",
            PlatformId = "674c1e0f-d334-dd32-b149-b79f424b479b",
            Retry = 3,
            SchemaVersion = "6.8.1.1",
            SecretKey = "Kentucky",
            ServiceId = "7fbfb540-6342-6e83-caaa-9557ec858a0b",
            TenantId = "5687f2ab-ea6d-7973-16b0-b5f0627a95d1",
            Timeout = 5000
        };
    }

    public static TransactionsAdapterMessage CreateMessage()
    {
        return new TransactionsAdapterMessage
        {
            Channel = "Channel",
            EventName = "EventName",
            Payload = """{ "payload_test": "test" }""",
            RequireRiskScore = true,
            SourceLob = "SourceLob",
            TransactionId = "TransactionId"
        };
    }
}
