namespace OddsCollector.DAL;

using Common;
using Data;
using Models;

/// <summary>
/// Provides access to odds data.
/// </summary>
public class DatabaseAdapter : IDatabaseAdapter, IDisposable
{
    private readonly ApplicationDatabaseContext _context;
    private bool _disposed;

    /// <summary>
    /// A constructor that is suitable for the dependency injection.
    /// </summary>
    /// <param name="context">An instance of <see cref="ApplicationDatabaseContext"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is null.</exception>
    public DatabaseAdapter(ApplicationDatabaseContext context)
    {
        ArgumentChecker.NullCheck(context, nameof(context));

        _context = context;
    }

    /// <summary>
    /// Saves a list of upcoming events.
    /// </summary>
    /// <param name="events">A list of upcoming events.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
    /// <exception cref="Exception">SportEvents are null.</exception>
    public async Task SaveUpcomingEventsAsync(IEnumerable<SportEvent> events)
    {
        ArgumentChecker.NullCheck(events, nameof(events));

        if (_context.SportEvents is null)
        {
            throw new Exception("SportEvents are null.");
        }

        var upcomingEvents = events.ToList();
        var existingEventIds = _context.SportEvents.Select(e => e.SportEventId);
        var eventsToSave = upcomingEvents.Where(e => !existingEventIds.Contains(e.SportEventId));

        _context.SportEvents.AddRange(eventsToSave);
        await _context.SaveChangesAsync();

        await SaveOddsAsync(upcomingEvents);
    }

    /// <summary>
    /// Saves a list of odds from given events.
    /// </summary>
    /// <param name="events">A list of events.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="events"/> is null.</exception>
    /// <exception cref="Exception">No odds for the given event.</exception>
    private async Task SaveOddsAsync(IEnumerable<SportEvent> events)
    {
        ArgumentChecker.NullCheck(events, nameof(events));

        var odds = events.SelectMany(e => e.Odds ?? new List<Odd>());

        if (_context.Odds is null)
        {
            throw new Exception("No odds for the given event.");
        }

        foreach (var o in odds)
        {
            var existing = _context.Odds.FirstOrDefault(
                x => x.SportEventId == o.SportEventId && x.LastUpdate == o.LastUpdate && x.Bookmaker == o.Bookmaker
            );

            if (existing is null)
            {
                _context.Odds.Add(o);
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves events with most recent odds.
    /// </summary>
    /// <returns>A list of events.</returns>
    /// <exception cref="Exception">
    /// There are no sport events available or
    /// There are no odds available.
    /// </exception>
    public IEnumerable<SportEvent> GetEventsWithLatestOdds()
    {
        if (_context.SportEvents is null)
        {
            throw new Exception("There are no sport events available.");
        }

        if (_context.Odds is null)
        { 
            throw new Exception("There are no odds available.");
        }

        var result = _context.SportEvents.ToList();

        foreach(var e in result)
        {
            var odds = _context.Odds.Where(o => o.SportEventId == e.SportEventId);

            var grouped = new Dictionary<string, Odd>();

            foreach(var o in odds)
            {
                if (string.IsNullOrEmpty(o.Bookmaker)) continue;
                    
                if (!grouped.ContainsKey(o.Bookmaker))
                {
                    grouped.Add(o.Bookmaker, o);
                }
                else
                {
                    if (grouped[o.Bookmaker].LastUpdate < o.LastUpdate)
                    {
                        grouped[o.Bookmaker] = o;
                    }
                }
            }
                
            e.Odds = grouped.Values.ToList();
        }

        return result;
    }

    /// <summary>
    /// Saves a list of event results.
    /// </summary>
    /// <param name="results">A list of event results.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="results"/> is null.</exception>
    /// <exception cref="Exception">SportEvents are null.</exception>
    public async Task SaveEventResultsAsync(Dictionary<string, string?> results)
    {
        ArgumentChecker.NullCheck(results, nameof(results));

        if (_context.SportEvents is null)
        {
            throw new Exception("SportEvents are null.");
        }

        foreach (var r in results)
        {
            var sportEvent = _context.SportEvents.FirstOrDefault(e => r.Key == e.SportEventId);

            if (sportEvent is null)
            {
                continue;
            }

            sportEvent.Outcome = r.Value;
            _context.Update(sportEvent);
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Disposes the object.
    /// </summary>
    /// <param name="disposing">Flag indicating whether disposal is in progress or not.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }
        _disposed = true;
    }

    /// <summary>
    /// Disposes the object.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}