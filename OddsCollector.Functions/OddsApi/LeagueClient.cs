using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OddsCollector.Functions.OddsApi.Configuration;

namespace OddsCollector.Functions.OddsApi;

/// <summary>
///     Asks The Odds API about every configured league in turn and joins the answers.
/// </summary>
/// <remarks>
///     A league that fails is logged and skipped, so one bad league does not cost the others. A run where every
///     league failed throws, so that an invalid API key or an exhausted quota shows up as a failed invocation
///     rather than a successful run with no data. When the host cancels the run, what the leagues already
///     answered is kept rather than thrown away.
/// </remarks>
/// <typeparam name="T">What is collected from each league.</typeparam>
internal abstract class LeagueClient<T>(ILogger logger, IOptions<OddsApiClientOptions> options)
{
    /// <summary>
    ///     What is collected, in the plural, for logs and errors. For example, "upcoming events".
    /// </summary>
    protected abstract string ItemsName { get; }

    /// <summary>
    ///     Gets what one league has. The result is enumerated once, inside the per-league error handling.
    /// </summary>
    protected abstract Task<IEnumerable<T>> GetLeagueAsync(string league, string apiKey,
        CancellationToken cancellationToken);

    protected async Task<T[]> CollectAsync(CancellationToken cancellationToken)
    {
        var leagues = options.Value.Leagues;
        var apiKey = options.Value.ApiKey;

        List<T> result = [];
        List<Exception> failures = [];

        foreach (var league in leagues)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var items = await GetLeagueAsync(league, apiKey, cancellationToken).ConfigureAwait(false);

                result.AddRange(items);
            }
            catch (Exception exception) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation(exception, "Collection was cancelled, keeping {Count} {Items} collected so far",
                    result.Count, ItemsName);

                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to get {Items} for {League}", ItemsName, league);
                failures.Add(exception);
            }
        }

        if (failures.Count > 0 && failures.Count == leagues.Count)
        {
            throw new AggregateException($"Failed to get {ItemsName} for every league", failures);
        }

        return [.. result];
    }
}
