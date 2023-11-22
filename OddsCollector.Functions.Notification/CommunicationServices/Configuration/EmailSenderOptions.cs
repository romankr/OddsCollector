using System.ComponentModel.DataAnnotations;

namespace OddsCollector.Functions.Notification.CommunicationServices.Configuration;

internal sealed class EmailSenderOptions
{
    [Required(AllowEmptyStrings = false)] public string RecipientAddress { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string SenderAddress { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string Subject { get; set; } = string.Empty;

    public void SetRecipientAddress(string? recipientAddress)
    {
        if (string.IsNullOrEmpty(recipientAddress))
        {
            throw new ArgumentException($"{nameof(recipientAddress)} cannot be null or empty",
                nameof(recipientAddress));
        }

        RecipientAddress = recipientAddress;
    }

    public void SetSenderAddress(string? senderAddress)
    {
        if (string.IsNullOrEmpty(senderAddress))
        {
            throw new ArgumentException($"{nameof(senderAddress)} cannot be null or empty", nameof(senderAddress));
        }

        SenderAddress = senderAddress;
    }

    public void SetSubject(string? subject)
    {
        if (string.IsNullOrEmpty(subject))
        {
            throw new ArgumentException($"{nameof(subject)} cannot be null or empty", nameof(subject));
        }

        Subject = subject;
    }
}
