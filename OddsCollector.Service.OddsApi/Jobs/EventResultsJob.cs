using OddsCollector.Service.OddsApi.Processor;
using Quartz;

namespace OddsCollector.Service.OddsApi.Jobs;

[DisallowConcurrentExecution]
internal sealed class EventResultsJob : IJob
{
    private readonly IEventProcessor _processor;

    public EventResultsJob(IEventProcessor? processor)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
    }

    public async Task Execute(IJobExecutionContext? context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await _processor.GetEventResults(context.CancellationToken).ConfigureAwait(false);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
