namespace OddsCollector.Service.OddsApi.Processor;

internal interface IEventProcessor
{
    Task GetEventResults(CancellationToken token);

    Task GetUpcomingEvents(CancellationToken token);
}
