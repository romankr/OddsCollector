using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;
using OddsCollector.Functions.Tests.Infrastructure.ServiceBus;
using FunctionApp = OddsCollector.Functions.Functions;

namespace OddsCollector.Functions.Tests.Tests.Functions;

internal sealed class PredictionFunction
{
    [Test]
    public void Run_WithServiceBusMessage_ReturnsPredictionAndLogsIt()
    {
        // Arrange
        var expectedPrediction = new EventPrediction { Id = "id", Winner = OutcomeTypes.HomeTeam };

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>()).Returns(expectedPrediction);

        var loggerMock = new FakeLogger<FunctionApp.PredictionFunction>();

        var function = new FunctionApp.PredictionFunction(loggerMock, strategyStub);

        // Act
        var prediction = function.Run(ServiceBusReceivedMessageFactory.CreateFromObject(new UpcomingEvent()));

        // Assert
        prediction.Should().BeSameAs(expectedPrediction);

        loggerMock.Collector.Count.Should().Be(1);
        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Information);
        loggerMock.LatestRecord.Message.Should().Be("Predicted HomeTeam for event id");
    }

    [Test]
    public void Run_WithException_LetsItReachTheHost()
    {
        // Arrange
        var expectedException = new Exception();

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>()).Throws(expectedException);

        var loggerMock = new FakeLogger<FunctionApp.PredictionFunction>();

        var function = new FunctionApp.PredictionFunction(loggerMock, strategyStub);

        // Act
        var action = () => function.Run(ServiceBusReceivedMessageFactory.CreateFromObject(new UpcomingEvent()));

        // Assert: the host abandons the message on a failure so it can be redelivered.
        // Swallowing would have it complete a message whose prediction was never stored.
        action.Should().Throw<Exception>().Which.Should().BeSameAs(expectedException);

        loggerMock.Collector.Count.Should().Be(0);
    }
}
