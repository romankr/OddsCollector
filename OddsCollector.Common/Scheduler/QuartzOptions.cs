namespace OddsCollector.Common.Scheduler;

internal sealed class QuartzOptions
{
    public Dictionary<string, string> Schedules { get; set; } = new Dictionary<string, string>();
}
