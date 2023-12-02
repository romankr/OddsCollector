using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.CommunicationServices;
using OddsCollector.Functions.CosmosDb;
using OddsCollector.Functions.Functions;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Functions;

[Parallelizable(ParallelScope.All)]
internal class NotificationFunctionTests
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var loggerStub = Substitute.For<ILogger<NotificationFunction>>();
        var emailSenderStub = Substitute.For<IEmailSender>();
        var cosmosDbClientStub = Substitute.For<ICosmosDbClient>();

        var function = new NotificationFunction(loggerStub, emailSenderStub, cosmosDbClientStub);

        function.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsException()
    {
        var emailSenderStub = Substitute.For<IEmailSender>();
        var cosmosDbClientStub = Substitute.For<ICosmosDbClient>();

        var action = () =>
        {
            _ = new NotificationFunction(null, emailSenderStub, cosmosDbClientStub);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Test]
    public void Constructor_WithNullEmailClient_ThrowsException()
    {
        var loggerStub = Substitute.For<ILogger<NotificationFunction>>();
        var cosmosDbClientStub = Substitute.For<ICosmosDbClient>();

        var action = () =>
        {
            _ = new NotificationFunction(loggerStub, null, cosmosDbClientStub);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("sender");
    }

    [Test]
    public void Constructor_WithNullCosmosDbClient_ThrowsException()
    {
        var loggerStub = Substitute.For<ILogger<NotificationFunction>>();
        var emailSenderStub = Substitute.For<IEmailSender>();

        var action = () =>
        {
            _ = new NotificationFunction(loggerStub, emailSenderStub, null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Test]
    public async Task Run_WithValidParameters_SendsEmails()
    {
        var loggerStub = Substitute.For<ILogger<NotificationFunction>>();

        var emailSenderMock = Substitute.For<IEmailSender>();

        IEnumerable<EventPrediction> predictons = [];
        var cosmosDbClientMock = Substitute.For<ICosmosDbClient>();
        cosmosDbClientMock.GetEventPredictionsAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(predictons));

        var function = new NotificationFunction(loggerStub, emailSenderMock, cosmosDbClientMock);

        var token = new CancellationToken();

        await function.Run(token).ConfigureAwait(false);

        await emailSenderMock.Received().SendEmailAsync(predictons, token);
        await cosmosDbClientMock.Received().GetEventPredictionsAsync(token);
    }

    [Test]
    public async Task Run_WithException_DoesntFail()
    {
        var loggerMock = Substitute.For<ILogger<NotificationFunction>>();

        var emailSenderMock = Substitute.For<IEmailSender>();

        var exception = new Exception();

        var cosmosDbClientMock = Substitute.For<ICosmosDbClient>();
        cosmosDbClientMock.GetEventPredictionsAsync(Arg.Any<CancellationToken>()).Throws(exception);

        var function = new NotificationFunction(loggerMock, emailSenderMock, cosmosDbClientMock);

        var token = new CancellationToken();

        await function.Run(token).ConfigureAwait(false);

        loggerMock.Received().LogError(exception, "Failed to send e-mail notification");
    }
}
