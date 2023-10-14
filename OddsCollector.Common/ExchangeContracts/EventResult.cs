namespace OddsCollector.Common.ExchangeContracts;

public class EventResult
{
    public string? Id { get; set; }
    public DateTime? CommenceTime { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? Winner { get; set; }
    public Guid? TraceId { get; set; }
}
