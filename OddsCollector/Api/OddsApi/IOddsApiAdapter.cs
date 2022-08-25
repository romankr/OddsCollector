namespace OddsCollector.Api.OddsApi;

using Models;

/// <summary>
/// Interface for classes that provide access to https://the-odds-api.com/.
/// </summary>
public interface IOddsApiAdapter
{
    /// <summary>
    /// Gets a list of upcoming football (soccer) events with odds for given list of european leagues.
    /// </summary>
    /// <param name="leagues">A list of european leagues.</param>
    /// <returns>A list of <see cref="SportEvent"/>.</returns>
    Task<IEnumerable<SportEvent>> GetUpcomingEventsAsync(IEnumerable<string> leagues);

    /// <summary>
    /// Gets a list of completed football (soccer) events with results for given list of european leagues.
    /// </summary>
    /// <param name="leagues">A list of european leagues.</param>
    /// <returns>A list of completed football (soccer) events.</returns>
    Task<Dictionary<string, string?>> GetCompletedEventsAsync(IEnumerable<string> leagues);
}