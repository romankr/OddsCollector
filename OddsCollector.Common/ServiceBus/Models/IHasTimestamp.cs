namespace OddsCollector.Common.ServiceBus.Models;

public interface IHasTimestamp
{
    DateTime? Timestamp { get; init; }
}
