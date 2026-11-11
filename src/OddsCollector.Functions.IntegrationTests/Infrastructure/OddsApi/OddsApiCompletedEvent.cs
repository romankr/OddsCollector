namespace OddsCollector.Functions.IntegrationTests.Infrastructure.OddsApi;

internal sealed record OddsApiCompletedEvent(
    string Id,
    DateTime CommenceTime,
    string HomeTeam,
    string AwayTeam,
    int HomeScore,
    int AwayScore);
