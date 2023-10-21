using System.Diagnostics.CodeAnalysis;
using Quartz;

namespace OddsCollector.Service.Notification.Jobs;

[DisallowConcurrentExecution]
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class NotificationJob : IJob
{
    private readonly IPredictionsProcessor _processor;

    public NotificationJob(IPredictionsProcessor processor)
    {
        _processor = processor;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await _processor.StartProcessingAsync().ConfigureAwait(false);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
