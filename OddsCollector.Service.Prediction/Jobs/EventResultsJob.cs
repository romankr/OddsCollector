using System.Diagnostics.CodeAnalysis;
using OddsCollector.Service.Prediction.Strategies;
using Quartz;

namespace OddsCollector.Service.Prediction.Jobs;

[DisallowConcurrentExecution]
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class EventResultsJob : IJob
{
    private readonly ILogger<EventResultsJob> _logger;
    private readonly IPredictionStrategy _strategy;

    public EventResultsJob(ILogger<EventResultsJob>? logger, IPredictionStrategy strategy)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    public async Task Execute(IJobExecutionContext? context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
