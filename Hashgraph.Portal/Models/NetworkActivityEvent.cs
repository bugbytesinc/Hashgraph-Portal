namespace Hashgraph.Portal.Models;

public class NetworkActivityEvent
{
    public NetworkActivityEventType Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int TryNo { get; set; } = 0;
    public string? Data { get; set; } = null;
}