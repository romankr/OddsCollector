using System.Diagnostics.CodeAnalysis;
using Quartz;

namespace OddsCollector.Service.Prediction.Jobs;

[DisallowConcurrentExecution]
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class PredictionJob : IJob
{
    private readonly IUpcomingEventsProcessor _processor;

    public PredictionJob(IUpcomingEventsProcessor processor)
    {
        _processor = processor;
    }

    public async Task Execute(IJobExecutionContext? context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await _processor.StartProcessingAsync().ConfigureAwait(false);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
