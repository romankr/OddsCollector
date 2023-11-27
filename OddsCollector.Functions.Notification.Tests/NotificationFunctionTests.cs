using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using NSubstitute;
using NUnit.Framework;
using OddsCollector.Common.Models;
using OddsCollector.Functions.Notification.CommunicationServices;
using OddsCollector.Functions.Notification.CosmosDb;

namespace OddsCollector.Functions.Notification.Tests;

internal class NotificationFunctionTests
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var emailSenderStub = Substitute.For<IEmailSender>();
        var cosmosDbClientStub = Substitute.For<ICosmosDbClient>();

        var function = new NotificationFunction(emailSenderStub, cosmosDbClientStub);

        function.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullEmailClient_ThrowsException()
    {
        var cosmosDbClientStub = Substitute.For<ICosmosDbClient>();

        var action = () =>
        {
            _ = new NotificationFunction(null, cosmosDbClientStub);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("sender");
    }

    [Test]
    public void Constructor_WithNullCosmosDbClient_ThrowsException()
    {
        var emailSenderStub = Substitute.For<IEmailSender>();

        var action = () =>
        {
            _ = new NotificationFunction(emailSenderStub, null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Test]
    public async Task Run_WithTimer_SendsEmails()
    {
        var timer = new TimerInfo();

        IEnumerable<EventPrediction> predictons = [];
        var cosmosDbClientMock = Substitute.For<ICosmosDbClient>();
        cosmosDbClientMock.GetEventPredictionsAsync().Returns(Task.FromResult(predictons));

        var emailSenderMock = Substitute.For<IEmailSender>();

        var function = new NotificationFunction(emailSenderMock, cosmosDbClientMock);

        await function.Run(timer).ConfigureAwait(false);

        await emailSenderMock.Received().SendEmailAsync(predictons);
        await cosmosDbClientMock.Received().GetEventPredictionsAsync();
    }
}
