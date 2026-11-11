namespace OddsCollector.Functions.IntegrationTests.Infrastructure.OddsApi;

internal sealed record OddsApiEvent(
    string Id,
    DateTime CommenceTime,
    string HomeTeam,
    string AwayTeam,
    IReadOnlyCollection<OddsApiBookmaker> Bookmakers);
