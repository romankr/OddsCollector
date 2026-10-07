using OddsCollector.Functions.Models;

namespace OddsCollector.Tests.Infrastructure.Models;

/// <summary>
///     Models with every required member set to a valid value, for tests that only need one to exist.
///     A test changes what it is about with a <c>with</c> expression.
/// </summary>
internal static class ValidModels
{
    public static readonly DateTime CommenceTime = new(2026, 9, 22, 18, 0, 0, DateTimeKind.Utc);

    public static Odd CreateOdd(double home = 2, double draw = 3, double away = 4)
    {
        return new Odd { Bookmaker = "bookmaker", Home = home, Draw = draw, Away = away };
    }

    public static UpcomingEvent CreateUpcomingEvent()
    {
        return new UpcomingEvent
        {
            Id = "id",
            HomeTeam = "homeTeam",
            AwayTeam = "awayTeam",
            CommenceTime = CommenceTime,
            Odds = [CreateOdd()]
        };
    }

    public static EventPrediction CreateEventPrediction()
    {
        return new EventPrediction
        {
            Id = "id",
            HomeTeam = "homeTeam",
            AwayTeam = "awayTeam",
            CommenceTime = CommenceTime,
            Outcome = OutcomeTypes.HomeTeam
        };
    }

    public static EventResult CreateEventResult()
    {
        return new EventResult { Id = "id", CommenceTime = CommenceTime, Outcome = OutcomeTypes.HomeTeam };
    }
}
