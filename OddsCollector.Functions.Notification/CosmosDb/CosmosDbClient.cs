using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using OddsCollector.Common.Models;
using OddsCollector.Functions.Notification.CosmosDb.Configuration;

namespace OddsCollector.Functions.Notification.CosmosDb;

internal sealed class CosmosDbClient : ICosmosDbClient
{
    private readonly Container _container;

    public CosmosDbClient(IOptions<CosmosDbOptions> options)
    {
        var client = new CosmosClient(options.Value.Connection);
        _container = client.GetContainer(options.Value.Database, options.Value.Container);
    }

    public async Task<IEnumerable<EventPrediction?>> GetEventPredictionsAsync()
    {
        var queryable = _container.GetItemLinqQueryable<EventPrediction>();

        // cosmosdb doesn't support grouping
        var matches = from prediction in queryable
            where prediction.CommenceTime >= DateTime.UtcNow
            select prediction;

        var cosmosdbresult = new List<EventPrediction>();

        using FeedIterator<EventPrediction> feed = matches.ToFeedIterator();

        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync().ConfigureAwait(false);

            cosmosdbresult.AddRange(response);
        }

        // so doing it manually
        var groups = cosmosdbresult.GroupBy(p => p.Id);

        return groups.Select(group => group.OrderByDescending(p => p.Timestamp).First()).ToList();
    }
}
