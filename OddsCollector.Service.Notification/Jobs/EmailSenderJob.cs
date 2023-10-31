using OddsCollector.Service.Notification.Email;
using Quartz;

namespace OddsCollector.Service.Notification.Jobs;

[DisallowConcurrentExecution]
internal sealed class EmailSenderJob : IJob
{
    private readonly IEmailSender _sender;

    public EmailSenderJob(IEmailSender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        await _sender.SendEmailAsync(context.CancellationToken).ConfigureAwait(false);

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
