namespace OddsCollector.DAL;

using Models;
using Data;

public class DatabaseAdapter : IDatabaseAdapter, IDisposable
{
    private readonly ApplicationDatabaseContext _context;
    private bool _disposed;

    public DatabaseAdapter(ApplicationDatabaseContext context)
    {
        _context = context;
    }

    public async Task SaveUpcomingEventsAsync(IEnumerable<SportEvent> events)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        if (_context.SportEvents is null)
        {
            throw new Exception("SportEvents are null");
        }

        var upcomingEvents = events.ToList();
        var existingEventIds = _context.SportEvents.Select(e => e.SportEventId);
        var eventsToSave = upcomingEvents.Where(e => !existingEventIds.Contains(e.SportEventId));

        _context.SportEvents.AddRange(eventsToSave);
        await _context.SaveChangesAsync();

        await SaveOddsAsync(upcomingEvents);
    }

    private async Task SaveOddsAsync(IEnumerable<SportEvent> events)
    {
        if (events is null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        var odds = events.SelectMany(e => e.Odds ?? new List<Odd>());

        if (_context.Odds is null) throw new Exception("Odds are null");

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

    public IEnumerable<SportEvent> GetEventsWithLatestOdds()
    {
        if (_context.SportEvents is null)
        {
            throw new Exception("SportEvents are null");
        }

        if (_context.Odds is null)
        { 
            throw new Exception("Odds are null");
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

    public async Task SaveEventResultsAsync(Dictionary<string, string?> results)
    {
        if (results is null)
        {
            throw new ArgumentNullException(nameof(results));
        }

        if (_context.SportEvents is null)
        {
            throw new Exception("SportEvents are null");
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}