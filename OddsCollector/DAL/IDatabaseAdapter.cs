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
    /// <param name="events">The list of upcoming events.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    Task SaveUpcomingEventsAsync(IEnumerable<SportEvent> events);

    /// <summary>
    /// Saves a list of event results.
    /// </summary>
    /// <param name="results">A list of event results.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    Task SaveEventResultsAsync(Dictionary<string, string?> results);

    /// <summary>
    /// Retrieves events with most recent odds.
    /// </summary>
    /// <returns>A list of events <see cref="IEnumerable{SportEvent}"/>.</returns>
    IEnumerable<SportEvent> GetEventsWithLatestOdds();
}