using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.Processors;

internal sealed class UpcomingEventsProcessor
{
    [Test]
    public async Task GetUpcomingEventsAsync_PassesCancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        var clientMock = Substitute.For<IOddsApiClient>();

        var processor = new OddsCollector.Functions.Processors.UpcomingEventsProcessor(clientMock);

        // Act
        await processor.GetUpcomingEventsAsync(cancellationToken);

        // Assert
        var calls = clientMock.ReceivedCalls().ToList();
        calls.Should().NotBeNullOrEmpty().And.HaveCount(1);

        var arguments = calls[0].GetArguments();
        arguments.Should().NotBeNullOrEmpty().And.HaveCount(1);

        arguments[0].Should().BeOfType<CancellationToken>().And.Be(cancellationToken);
    }
}
