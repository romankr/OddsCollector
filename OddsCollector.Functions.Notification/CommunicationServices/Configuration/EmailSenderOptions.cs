using System.ComponentModel.DataAnnotations;

namespace OddsCollector.Functions.Notification.CommunicationServices.Configuration;

internal sealed class EmailSenderOptions
{
    [Required(AllowEmptyStrings = false)] public string RecipientAddress { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string SenderAddress { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string Subject { get; set; } = string.Empty;

    public void SetRecipientAddress(string? recipientAddress)
    {
        ArgumentException.ThrowIfNullOrEmpty(recipientAddress);

        RecipientAddress = recipientAddress;
    }

    public void SetSenderAddress(string? senderAddress)
    {
        ArgumentException.ThrowIfNullOrEmpty(senderAddress);

        SenderAddress = senderAddress;
    }

    public void SetSubject(string? subject)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);

        Subject = subject;
    }
}
