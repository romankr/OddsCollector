namespace OddsCollector.Api.OddsApi;

using Models;

public interface IOddsApiAdapter
{
    Task<IEnumerable<SportEvent>> GetUpcomingEventsAsync(IEnumerable<string> leagues);

    Task<Dictionary<string, string?>> GetCompletedEventsAsync(IEnumerable<string> leagues);
}