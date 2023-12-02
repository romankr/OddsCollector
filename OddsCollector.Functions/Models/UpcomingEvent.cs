namespace OddsCollector.Functions.Models;

public class UpcomingEvent
{
    public string AwayTeam { get; set; } = string.Empty;
    public DateTime CommenceTime { get; set; } = DateTime.MinValue;
    public string HomeTeam { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public IEnumerable<Odd> Odds { get; set; } = [];
    public DateTime Timestamp { get; set; } = DateTime.MinValue;
    public Guid TraceId { get; set; } = Guid.Empty;
}
