using OddsCollector.Common.Models;

namespace OddsCollector.Functions.Notification.CosmosDb;

internal interface ICosmosDbClient
{
    Task<IEnumerable<EventPrediction>> GetEventPredictionsAsync();
}
