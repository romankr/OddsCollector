using System.ComponentModel.DataAnnotations;

namespace OddsCollector.Functions.Notification.CommunicationServices.Configuration;

internal sealed class EmailSenderOptions
{
    [Required(AllowEmptyStrings = false)] public string Connection { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string RecipientAddress { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string SenderAddress { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string Subject { get; set; } = string.Empty;
}
