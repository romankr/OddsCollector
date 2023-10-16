namespace OddsCollector.Common.ExchangeContracts;

public class UpcomingEvent
{
    public string? AwayTeam { get; init; }
    public DateTime? CommenceTime { get; init; }
    public string? HomeTeam { get; init; }
    public string? Id { get; init; }
    public IEnumerable<Odd?>? Odds { get; init; }
    public DateTime? Timestamp { get; init; }
    public Guid? TraceId { get; init; }
}
