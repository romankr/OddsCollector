using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.Processors;

internal sealed class EventResultProcessor
{
    [Test]
    public async Task GetEventResultsAsync_PassesTraceId()
    {
        // Arrange
        var clientMock = Substitute.For<IOddsApiClient>();

        var processor = new OddsCollector.Functions.Processors.EventResultProcessor(clientMock);

        // Act
        await processor.GetEventResultsAsync(new CancellationToken());

        // Assert
        var calls = clientMock.ReceivedCalls().ToList();
        calls.Should().NotBeNullOrEmpty().And.HaveCount(1);

        var arguments = calls[0].GetArguments();
        arguments.Should().NotBeNullOrEmpty().And.HaveCount(3);

        arguments[0].Should().BeOfType<Guid>().And.NotBe(Guid.Empty);
    }

    [Test]
    public async Task GetEventResultsAsync_PassesDateTime()
    {
        // Arrange
        var clientMock = Substitute.For<IOddsApiClient>();

        var processor = new OddsCollector.Functions.Processors.EventResultProcessor(clientMock);

        // Act
        await processor.GetEventResultsAsync(new CancellationToken());

        // Assert
        var calls = clientMock.ReceivedCalls().ToList();
        calls.Should().NotBeNullOrEmpty().And.HaveCount(1);

        var arguments = calls[0].GetArguments();
        arguments.Should().NotBeNullOrEmpty().And.HaveCount(3);
        arguments[1].Should().BeOfType<DateTime>();

#pragma warning disable CS8605 // Unboxing a possibly null value.
        var timestamp = (DateTime)arguments[1];
#pragma warning restore CS8605 // Unboxing a possibly null value.

        timestamp.Should().BeCloseTo(DateTime.UtcNow, new TimeSpan(0, 0, 5));
    }

    [Test]
    public async Task GetEventResultsAsync_PassesCancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        var clientMock = Substitute.For<IOddsApiClient>();

        var processor = new OddsCollector.Functions.Processors.EventResultProcessor(clientMock);

        // Act
        await processor.GetEventResultsAsync(cancellationToken);

        // Assert
        var calls = clientMock.ReceivedCalls().ToList();
        calls.Should().NotBeNullOrEmpty().And.HaveCount(1);

        var arguments = calls[0].GetArguments();
        arguments.Should().NotBeNullOrEmpty().And.HaveCount(3);

        arguments[2].Should().BeOfType<CancellationToken>().And.Be(cancellationToken);
    }
}
