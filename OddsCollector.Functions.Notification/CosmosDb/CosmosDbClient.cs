using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using OddsCollector.Common.Models;

namespace OddsCollector.Functions.Notification.CosmosDb;

internal sealed class CosmosDbClient(Container? container) : ICosmosDbClient
{
    private readonly Container _container = container ?? throw new ArgumentNullException(nameof(container));

    public async Task<IEnumerable<EventPrediction>> GetEventPredictionsAsync(CancellationToken cancellationToken)
    {
        var queryable = _container.GetItemLinqQueryable<EventPrediction>();

        // cosmosdb doesn't support grouping
        var matches = from prediction in queryable
                      where prediction.CommenceTime >= DateTime.UtcNow
                      select prediction;

        List<EventPrediction> cosmosdbresult = [];

        using FeedIterator<EventPrediction> feed = matches.ToFeedIterator();

        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync(cancellationToken).ConfigureAwait(false);

            cosmosdbresult.AddRange(response);
        }

        // so doing it manually
        var groups = cosmosdbresult.GroupBy(p => p.Id);

        return groups.Select(group => group.OrderByDescending(p => p.Timestamp).First()).ToList();
    }
}
