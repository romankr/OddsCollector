namespace OddsCollector.Functions.Models;

internal sealed class UpcomingEvent
{
    public string AwayTeam { get; set; } = string.Empty;
    public DateTime CommenceTime { get; set; } = DateTime.MinValue;
    public string HomeTeam { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public IEnumerable<Odd> Odds { get; set; } = [];
}
