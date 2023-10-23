namespace OddsCollector.Common.ServiceBus.Models;

public class EventResult : IHasTraceId, IHasTimestamp
{
    public DateTime? CommenceTime { get; init; }
    public string? Id { get; init; }
    public DateTime? Timestamp { get; init; }
    public Guid? TraceId { get; init; }
    public string? Winner { get; init; }
}
