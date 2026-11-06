using System.Diagnostics.Metrics;

namespace OddsCollector.Functions.OddsApi;

internal sealed class OddsApiMetrics
{
    public const string MeterName = "OddsCollector.Functions.OddsApi";
    public const string LeagueFailuresInstrumentName = "oddscollector.oddsapi.league.failures";

    public const string OperationTag = "operation";
    public const string LeagueTag = "league";

    public const string UpcomingEventsOperation = "upcoming-events";
    public const string EventResultsOperation = "event-results";

    private readonly Counter<long> _leagueFailures;

    public OddsApiMetrics(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);

        _leagueFailures = meterFactory.Create(MeterName).CreateCounter<long>(LeagueFailuresInstrumentName,
            unit: "{failure}",
            description: "Number of times the data of a league could not be collected from The Odds API");
    }

    public void RecordLeagueFailure(string operation, string league)
    {
        _leagueFailures.Add(1,
            new KeyValuePair<string, object?>(OperationTag, operation),
            new KeyValuePair<string, object?>(LeagueTag, league));
    }
}
