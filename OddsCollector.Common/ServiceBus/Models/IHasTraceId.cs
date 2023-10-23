namespace OddsCollector.Common.ServiceBus.Models;

public interface IHasTraceId
{
    Guid? TraceId { get; init; }
}
