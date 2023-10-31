namespace OddsCollector.Service.OddsApi.Processor;

internal sealed class EventProcessorOptions
{
    public HashSet<string> Leagues { get; set; } = new HashSet<string>();
}
