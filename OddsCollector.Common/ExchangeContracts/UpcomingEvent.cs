namespace OddsCollector.Common.ExchangeContracts;

public class UpcomingEvent
{
    public string? Id { get; set; }
    public DateTime? CommenceTime { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? HomeTeam { get; set; }
    public string? AwayTeam { get; set; }
    public IEnumerable<Odd?>? Odds { get; set; }
    public Guid? TraceId { get; set; }
}