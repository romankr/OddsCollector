namespace OddsCollector.Api.OddsApi;

using Models;

/// <summary>
/// Interface for objects that convert objects received from Odds API.
/// </summary>
public interface IOddsApiObjectConverter
{
    /// <summary>
    /// Converts events.
    /// </summary>
    /// <param name="events">A list of <see cref="Anonymous2"/> objects.</param>
    /// <returns>A list of <see cref="SportEvent"/> objects.</returns>
    IEnumerable<SportEvent> ToSportEvents(ICollection<Anonymous2>[]? events);

    /// <summary>
    /// Converts an <see cref="Anonymous2"/> event into <see cref="SportEvent"/>.
    /// </summary>
    /// <param name="input">An <see cref="Anonymous2"/> event.</param>
    /// <returns>A <see cref="SportEvent"/> instance.</returns>
    SportEvent ToSportEvent(Anonymous2 input);

    /// <summary>
    /// Converts a list of <see cref="Bookmakers"/> into a list of <see cref="Odd"/>.
    /// </summary>
    /// <param name="bookmakers">A list of <see cref="Bookmakers"/>.</param>
    /// <param name="sportEvent">A parent <see cref="Bookmakers"/> instance.</param>
    /// <returns>A list of <see cref="Odd"/>.</returns>
    IEnumerable<Odd> ToOdds(ICollection<Bookmakers> bookmakers, SportEvent sportEvent);

    /// <summary>
    /// Converts odds.
    /// </summary>
    /// <param name="bookmaker">A <see cref="Bookmakers"/> object.</param>
    /// <param name="sportEvent">A <see cref="SportEvent"/> object.</param>
    /// <returns>A <see cref="Odd"/> object.</returns>
    Odd ToOdd(Bookmakers bookmaker, SportEvent sportEvent);

    /// <summary>
    /// Converts a list of outcomes.
    /// </summary>
    /// <param name="bookmaker">A <see cref="Bookmakers"/> object.</param>
    /// <returns>A dictionary with outcomes.</returns>
    Dictionary<string, double> ToOutcomes(Bookmakers bookmaker);

    /// <summary>
    /// Converts completed events.
    /// </summary>
    /// <param name="events">A list of event lists.</param>
    /// <returns>A list fo completed events.</returns>
    Dictionary<string, string?> ToCompletedEvents(ICollection<Anonymous3>[]? events);

    /// <summary>
    /// Converts completed events.
    /// </summary>
    /// <param name="events">A list fo events.</param>
    /// <returns>A list of completed events.</returns>
    Dictionary<string, string?> ToCompletedEvents(IEnumerable<Anonymous3>? events);

    /// <summary>
    /// Converts a completed event <see cref="Anonymous3"/> to a result string with the winning team (or draw).
    /// </summary>
    /// <param name="input">A completed event <see cref="Anonymous3"/>.</param>
    /// <returns>A pair of id and result with the winning team (or draw).</returns>
    KeyValuePair<string, string?> ToEventResultPair(Anonymous3 input);

    /// <summary>
    /// Converts <see cref="ScoreModel"/> to a score pair object.
    /// </summary>
    /// <param name="score">A <see cref="ScoreModel"/> object.</param>
    /// <returns>A score pair object.</returns>
    KeyValuePair<string, int> ToScorePair(ScoreModel score);
}