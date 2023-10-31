using OddsCollector.Service.Notification.ServiceBus;
using Quartz;

namespace OddsCollector.Service.Notification.Jobs;

[DisallowConcurrentExecution]
internal sealed class ServiceBusReaderJob : IJob
{
    private readonly IPredictionsProcessor _processor;

    public ServiceBusReaderJob(IPredictionsProcessor processor)
    {
        _processor = processor;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await _processor.StartProcessingAsync(context.CancellationToken).ConfigureAwait(false);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
