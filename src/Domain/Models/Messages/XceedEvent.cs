namespace Domain.Models.Messages;

public class XceedEvent
{
    public GaEvent GAEvent { get; set; } = new();
}
