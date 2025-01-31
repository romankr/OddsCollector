using Microsoft.Azure.Functions.Worker;
using NSubstitute.ReceivedExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.Predictions;
using OddsCollector.Functions.Tests.Infrastructure.ServiceBus;

namespace OddsCollector.Functions.Tests.Tests.Processors;

internal sealed class PredictionProcessor
{
    [Test]
    public async Task DeserializeAndCompleteMessageAsync_WithServiceBusMessage_ReturnsPredictionAndCompletesMessage()
    {
        // Arrange
        var actionsMock = Substitute.For<ServiceBusMessageActions>();

        var expectedPrediction = new EventPrediction();
        var message = ServiceBusReceivedMessageFactory.CreateFromObject(new UpcomingEvent());
        var strategyStub = Substitute.For<IPredictionStrategy>();
        strategyStub.GetPrediction(Arg.Any<UpcomingEvent>()).Returns(expectedPrediction);

        var cancellationToken = new CancellationToken();

        var processor = new OddsCollector.Functions.Processors.PredictionProcessor(strategyStub);

        // Act
        var prediction = await processor.DeserializeAndCompleteMessageAsync(message, actionsMock, cancellationToken).ConfigureAwait(false);

        // Assert
        prediction.Should().NotBeNull().And.Be(expectedPrediction);

        await actionsMock.Received(Quantity.Exactly(1)).CompleteMessageAsync(message, cancellationToken);
    }
}
