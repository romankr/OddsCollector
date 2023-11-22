using System.Text.Json;
using Azure;
using Azure.Communication.Email;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using OddsCollector.Common.Models;
using OddsCollector.Functions.Notification.CommunicationServices;
using OddsCollector.Functions.Notification.CommunicationServices.Configuration;

namespace OddsCollector.Functions.Notification.Tests.CommunicationServices;

internal class EmailSenderTests
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var optionsStub = Substitute.For<IOptions<EmailSenderOptions>>();
        optionsStub.Value.Returns(new EmailSenderOptions());
        var clientStub = Substitute.For<EmailClient>();

        var sender = new EmailSender(optionsStub, clientStub);

        sender.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullOptions_ThrowsException()
    {
        var clientStub = Substitute.For<EmailClient>();

        var action = () =>
        {
            _ = new EmailSender(null, clientStub);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Test]
    public void Constructor_WithNullEmailSender_ThrowsException()
    {
        var optionsStub = Substitute.For<IOptions<EmailSenderOptions>>();
        optionsStub.Value.Returns(new EmailSenderOptions());

        var action = () =>
        {
            _ = new EmailSender(optionsStub, null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Test]
    public async Task SendEmailAsync_WithValidOptions_SendsEmails()
    {
        var predictions = new List<EventPrediction>
        {
            new()
            {
                AwayTeam = "away",
                Bookmaker = "bookmaker",
                CommenceTime = DateTime.UtcNow,
                HomeTeam = "Home",
                Id = "id",
                Strategy = "strategy",
                Timestamp = DateTime.UtcNow,
                TraceId = Guid.NewGuid(),
                Winner = "winner"
            }
        };

        const string recipientAddress = "test@example.com";
        const string senderAddress = "test2@example.com";
        const string subject = "test";

        var optionsStub = Substitute.For<IOptions<EmailSenderOptions>>();
        optionsStub.Value.Returns(new EmailSenderOptions
        {
            RecipientAddress = recipientAddress, SenderAddress = senderAddress, Subject = subject
        });

        var clientMock = Substitute.For<EmailClient>();

        var sender = new EmailSender(optionsStub, clientMock);

        await sender.SendEmailAsync(predictions).ConfigureAwait(false);

        var received = clientMock.ReceivedCalls().ToList();

        received.Should().NotBeNull();
        received.Should().HaveCount(1);

        var firstReceived = received.First();

        firstReceived.GetMethodInfo().Name.Should().Be("SendAsync");

        var firstReceivedArguments = firstReceived.GetArguments();

        firstReceivedArguments.Should().NotBeNull();
        firstReceivedArguments[0].Should().Be(WaitUntil.Completed);
        firstReceivedArguments[1].Should().Be(senderAddress);
        firstReceivedArguments[2].Should().Be(recipientAddress);
        firstReceivedArguments[3].Should().Be(subject);

        var deserialized = JsonSerializer.Deserialize<List<EventPrediction>>((string)firstReceivedArguments[4]!);

        deserialized![0].Id.Should().Be(predictions[0].Id);
    }
}
