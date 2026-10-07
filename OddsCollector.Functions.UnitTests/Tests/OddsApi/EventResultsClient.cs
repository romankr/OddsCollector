using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.OddsApi;

internal sealed class EventResultsClient
{
    private const string ApiKey = "apiKey";
    private const string League = "league";

    // The client asks The Odds API for the scores of the last three days.
    private const int DaysFrom = 3;

    [Test]
    public async Task GetEventResultsAsync_WithLeague_ReturnsConvertedEventResults()
    {
        // Arrange
        ICollection<Anonymous3> originalEvents = [new()];
        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub.ScoresAsync(League, ApiKey, DaysFrom, Arg.Any<CancellationToken>()).Returns(originalEvents);

        EventResult[] eventResults = [new()];
        var converterStub = Substitute.For<IOriginalCompletedEventConverter>();
        converterStub.ToEventResults(originalEvents).Returns(eventResults);

        var client = CreateClient([League], webApiClientStub, converterStub);

        // Act
        var results = await client.GetEventResultsAsync(CancellationToken.None);

        // Assert
        results.Should().Equal(eventResults);
    }

    [Test]
    public async Task GetEventResultsAsync_WithRequestedCancellation_ThrowsRatherThanReturningPart()
    {
        // Arrange
        var client = CreateClient([League], Substitute.For<IClient>(),
            Substitute.For<IOriginalCompletedEventConverter>());

        // Act
        var action = () => client.GetEventResultsAsync(new CancellationToken(canceled: true));

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task GetEventResultsAsync_WithFailingLeague_SkipsItAndLogsError()
    {
        // Arrange
        const string failingLeague = nameof(failingLeague);
        const string workingLeague = nameof(workingLeague);

        var expectedException = new HttpRequestException();

        ICollection<Anonymous3> originalEvents = [new()];
        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub.ScoresAsync(failingLeague, ApiKey, DaysFrom, Arg.Any<CancellationToken>())
            .Throws(expectedException);
        webApiClientStub.ScoresAsync(workingLeague, ApiKey, DaysFrom, Arg.Any<CancellationToken>())
            .Returns(originalEvents);

        EventResult[] eventResults = [new()];
        var converterStub = Substitute.For<IOriginalCompletedEventConverter>();
        converterStub.ToEventResults(originalEvents).Returns(eventResults);

        var loggerMock = new FakeLogger<FunctionApp.EventResultsClient>();

        var client = CreateClient([failingLeague, workingLeague], webApiClientStub, converterStub, loggerMock);

        // Act
        var results = await client.GetEventResultsAsync(CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        results.Should().Equal(eventResults);

        var record = loggerMock.Collector.GetSnapshot().Should().ContainSingle().Subject;
        record.Level.Should().Be(LogLevel.Error);
        record.Message.Should().Be($"Failed to get event results for {failingLeague}");
        record.Exception.Should().BeSameAs(expectedException);
    }

    private static FunctionApp.EventResultsClient CreateClient(HashSet<string> leagues, IClient webApiClient,
        IOriginalCompletedEventConverter converter, ILogger<FunctionApp.EventResultsClient>? logger = null)
    {
        var options = Options.Create(new OddsApiClientOptions { Leagues = leagues, ApiKey = ApiKey });

        return new FunctionApp.EventResultsClient(logger ?? NullLogger<FunctionApp.EventResultsClient>.Instance,
            options, webApiClient, converter);
    }
}
