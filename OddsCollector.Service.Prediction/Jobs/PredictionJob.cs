using OddsCollector.Service.Prediction.ServiceBus;
using Quartz;

namespace OddsCollector.Service.Prediction.Jobs;

[DisallowConcurrentExecution]
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

        await _processor.StartProcessingAsync(context.CancellationToken).ConfigureAwait(false);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
