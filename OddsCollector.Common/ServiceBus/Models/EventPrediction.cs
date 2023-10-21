namespace OddsCollector.Common.ServiceBus.Models;

public class EventPrediction
{
    public Odd? BestOdd { get; init; }
    public string? Id { get; init; }
    public string? Strategy { get; init; }
    public DateTime? Timestamp { get; init; }
    public Guid? TraceId { get; init; }
    public string? Winner { get; init; }
}
