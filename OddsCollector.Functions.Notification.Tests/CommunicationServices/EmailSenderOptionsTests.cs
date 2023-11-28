using FluentAssertions;
using OddsCollector.Functions.Notification.CommunicationServices.Configuration;

namespace OddsCollector.Functions.Notification.Tests.CommunicationServices;

[Parallelizable(ParallelScope.All)]
internal class EmailSenderOptionsTests
{
    [Test]
    public void SetRecipientAddress_WithValidAddress_ReturnsValidInstance()
    {
        const string address = "test@example.com";

        var options = new EmailSenderOptions();

        options.SetRecipientAddress(address);

        options.RecipientAddress.Should().Be(address);
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetRecipientAddress_WithNullOrEmptyRecipientAddress_ThrowsException(string? recipientAddress)
    {
        var options = new EmailSenderOptions();

        var action = () =>
        {
            options.SetRecipientAddress(recipientAddress);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(recipientAddress));
    }

    [Test]
    public void SetSenderAddress_WithValidSenderAddress_ReturnsValidInstance()
    {
        const string address = "test@example.com";

        var options = new EmailSenderOptions();

        options.SetSenderAddress(address);

        options.SenderAddress.Should().Be(address);
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetSenderAddress_WithNullOrEmptySenderAddress_ThrowsException(string? senderAddress)
    {
        var options = new EmailSenderOptions();

        var action = () =>
        {
            options.SetSenderAddress(senderAddress);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(senderAddress));
    }

    [Test]
    public void SetSubject_WithValidSubject_ReturnsValidInstance()
    {
        const string subject = nameof(subject);

        var options = new EmailSenderOptions();

        options.SetSubject(subject);

        options.Subject.Should().Be(subject);
    }

    [TestCase("")]
    [TestCase(null)]
    public void SetSubject_WithNullOrEmptySubject_ThrowsException(string? subject)
    {
        var options = new EmailSenderOptions();

        var action = () =>
        {
            options.SetSubject(subject);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(subject));
    }
}
