using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.Processors;

internal sealed class UpcomingEventsProcessor
{
    [Test]
    public async Task GetUpcomingEventsAsync_PassesTraceId()
    {
        // Arrange
        var clientMock = Substitute.For<IOddsApiClient>();

        var processor = new OddsCollector.Functions.Processors.UpcomingEventsProcessor(clientMock);

        // Act
        await processor.GetUpcomingEventsAsync(new CancellationToken());

        // Assert
        var calls = clientMock.ReceivedCalls().ToList();
        calls.Should().NotBeNullOrEmpty().And.HaveCount(1);

        var arguments = calls[0].GetArguments();
        arguments.Should().NotBeNullOrEmpty().And.HaveCount(3);

        arguments[0].Should().BeOfType<Guid>().And.NotBe(Guid.Empty);
    }

    [Test]
    public async Task GetUpcomingEventsAsync_PassesTimestamp()
    {
        // Arrange
        var clientMock = Substitute.For<IOddsApiClient>();

        var processor = new OddsCollector.Functions.Processors.UpcomingEventsProcessor(clientMock);

        // Act
        await processor.GetUpcomingEventsAsync(new CancellationToken());

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
        arguments.Should().NotBeNullOrEmpty().And.HaveCount(3);

        arguments[2].Should().BeOfType<CancellationToken>().And.Be(cancellationToken);
    }
}
