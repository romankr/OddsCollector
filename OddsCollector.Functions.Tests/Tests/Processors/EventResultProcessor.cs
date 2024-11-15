using OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.Processors;

internal class EventResultProcessor
{
    [Test]
    public async Task PassesTraceId()
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
    public async Task PassesDateTime()
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

        arguments[1].Should().BeOfType<DateTime>().And.NotBe(DateTime.MinValue).And.NotBe(DateTime.MaxValue);
    }

    [Test]
    public async Task PassesCancellationToken()
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
