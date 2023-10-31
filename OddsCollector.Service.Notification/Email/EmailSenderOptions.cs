namespace OddsCollector.Service.Notification.Email;

internal sealed class EmailSenderOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public string SenderAddress { get; set; } = string.Empty;

    public string RecipientAddress { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;
}
