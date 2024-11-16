using Microsoft.Azure.Functions.Worker;
using NSubstitute.ReceivedExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Strategies;
using OddsCollector.Functions.Tests.Infrastructure.Data;
using OddsCollector.Functions.Tests.Infrastructure.ServiceBus;

namespace OddsCollector.Functions.Tests.Tests.Processors;

internal class PredictionProcessor
{
    [Test]
    public async Task DeserializeAndCompleteMessageAsync_WithServiceBusMessage_ReturnsPredictionAndCompletesMessage()
    {
        // Arrange
        var actionsMock = Substitute.For<ServiceBusMessageActions>();

        var cancellationToken = new CancellationToken();

        var upcomingEvent = new UpcomingEventBuilder().SetSampleData().Instance;
        var expectedPrediction = new EventPredictionBuilder().SetSampleData().Instance;

        var message = ServiceBusReceivedMessageFactory.CreateFromObject(upcomingEvent);

        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>(), Arg.Any<DateTime>())
            .Returns(expectedPrediction);

        var processor = new OddsCollector.Functions.Processors.PredictionProcessor(strategyStub);

        // Act
        var prediction = await processor.DeserializeAndCompleteMessageAsync(message, actionsMock, cancellationToken).ConfigureAwait(false);

        // Assert
        prediction.Should().NotBeNull().And.Be(expectedPrediction);

        await actionsMock.Received(Quantity.Exactly(1)).CompleteMessageAsync(message, cancellationToken);
    }
}
