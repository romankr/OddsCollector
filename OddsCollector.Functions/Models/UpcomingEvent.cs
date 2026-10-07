namespace OddsCollector.Functions.Models;

internal sealed record UpcomingEvent
{
    public required string AwayTeam { get; init => field = Guard.NotBlank(value, nameof(AwayTeam)); }

    public required DateTime CommenceTime { get; init => field = UtcDateTime.Require(value, nameof(CommenceTime)); }

    public required string HomeTeam { get; init => field = Guard.NotBlank(value, nameof(HomeTeam)); }

    public required string Id { get; init => field = Guard.NotBlank(value, nameof(Id)); }

    public required IEnumerable<Odd> Odds { get; init => field = Guard.NotNull(value, nameof(Odds)); }
}
