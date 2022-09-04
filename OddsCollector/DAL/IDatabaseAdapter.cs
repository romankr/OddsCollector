namespace OddsCollector.DAL;

using Models;

/// <summary>
/// Interface for objects that provide access to odds data.
/// </summary>
public interface IDatabaseAdapter
{
    /// <summary>
    /// Saves a list of upcoming events.
    /// </summary>
    /// <param name="events">A list of upcoming events.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    Task SaveUpcomingEventsAsync(IEnumerable<SportEvent> events);

    /// <summary>
    /// Saves a list of event results.
    /// </summary>
    /// <param name="results">A list of event results.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    Task SaveEventResultsAsync(Dictionary<string, string?> results);

    /// <summary>
    /// Retrieves events with limited number of odds - only the most recent ones.
    /// </summary>
    /// <returns>A list of events.</returns>
    IEnumerable<SportEvent> GetEventsWithLatestOdds();
}