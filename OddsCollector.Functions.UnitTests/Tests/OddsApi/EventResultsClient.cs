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
using OddsCollector.Tests.Infrastructure.Models;
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

        EventResult[] eventResults = [ValidModels.CreateEventResult()];
        var converterStub = Substitute.For<IOriginalCompletedEventConverter>();
        converterStub.ToEventResults(originalEvents).Returns(eventResults);

        var client = CreateClient([League], webApiClientStub, converterStub);

        // Act
        var results = await client.GetEventResultsAsync(CancellationToken.None);

        // Assert
        results.Should().Equal(eventResults);
    }

    [Test]
    public async Task GetEventResultsAsync_WithCancellationBeforeFirstLeague_ReturnsNothingWithoutCallingApi()
    {
        // Arrange
        var webApiClientMock = Substitute.For<IClient>();

        var loggerMock = new FakeLogger<FunctionApp.EventResultsClient>();

        var client = CreateClient([League], webApiClientMock, Substitute.For<IOriginalCompletedEventConverter>(), loggerMock);

        // Act
        var results = await client.GetEventResultsAsync(new CancellationToken(canceled: true));

        // Assert
        using var scope = new AssertionScope();

        results.Should().BeEmpty();
        webApiClientMock.ReceivedCalls().Should().BeEmpty();
        loggerMock.Collector.GetSnapshot().Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Level = LogLevel.Information,
            Message = "Collection was cancelled, keeping 0 event results collected so far"
        });
    }

    [Test]
    public async Task GetEventResultsAsync_WithCancellationDuringLeague_KeepsResultsOfEarlierLeagues()
    {
        // Arrange
        const string answeredLeague = nameof(answeredLeague);
        const string cancelledLeague = nameof(cancelledLeague);
        const string skippedLeague = nameof(skippedLeague);

        using var cancellation = new CancellationTokenSource();

        ICollection<Anonymous3> originalEvents = [new()];
        var webApiClientMock = Substitute.For<IClient>();
        webApiClientMock.ScoresAsync(answeredLeague, ApiKey, DaysFrom, Arg.Any<CancellationToken>())
            .Returns(originalEvents);
        webApiClientMock.ScoresAsync(cancelledLeague, ApiKey, DaysFrom, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                // The host cancels the run while the request is in flight.
                cancellation.Cancel();
                return Task.FromCanceled<ICollection<Anonymous3>>(cancellation.Token);
            });

        EventResult[] converted = [ValidModels.CreateEventResult()];
        var converterStub = Substitute.For<IOriginalCompletedEventConverter>();
        converterStub.ToEventResults(originalEvents).Returns(converted);

        var loggerMock = new FakeLogger<FunctionApp.EventResultsClient>();

        var client = CreateClient([answeredLeague, cancelledLeague, skippedLeague], webApiClientMock,
            converterStub, loggerMock);

        // Act
        var results = await client.GetEventResultsAsync(cancellation.Token);

        // Assert
        using var scope = new AssertionScope();

        results.Should().Equal(converted);
        await webApiClientMock.DidNotReceive().ScoresAsync(skippedLeague, ApiKey, DaysFrom, Arg.Any<CancellationToken>());
        loggerMock.Collector.GetSnapshot().Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Level = LogLevel.Information,
            Message = "Collection was cancelled, keeping 1 event results collected so far"
        });
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

        EventResult[] eventResults = [ValidModels.CreateEventResult()];
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

    [Test]
    public async Task GetEventResultsAsync_WithEveryLeagueFailing_ThrowsWithEveryFailure()
    {
        // Arrange
        const string firstLeague = nameof(firstLeague);
        const string secondLeague = nameof(secondLeague);

        var firstException = new HttpRequestException();
        var secondException = new HttpRequestException();

        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub.ScoresAsync(firstLeague, ApiKey, DaysFrom, Arg.Any<CancellationToken>())
            .Throws(firstException);
        webApiClientStub.ScoresAsync(secondLeague, ApiKey, DaysFrom, Arg.Any<CancellationToken>())
            .Throws(secondException);

        var client = CreateClient([firstLeague, secondLeague], webApiClientStub,
            Substitute.For<IOriginalCompletedEventConverter>());

        // Act
        var action = () => client.GetEventResultsAsync(CancellationToken.None);

        // Assert
        var exception = (await action.Should().ThrowExactlyAsync<AggregateException>()).Which;
        exception.InnerExceptions.Should().Equal(firstException, secondException);
        exception.Message.Should().StartWith("Failed to get event results for every league");
    }

    private static FunctionApp.EventResultsClient CreateClient(HashSet<string> leagues, IClient webApiClient,
        IOriginalCompletedEventConverter converter, ILogger<FunctionApp.EventResultsClient>? logger = null)
    {
        var clientOptions = new OddsApiClientOptions { ApiKey = ApiKey };
        clientOptions.AddLeagues(string.Join(';', leagues));

        var options = Options.Create(clientOptions);

        return new FunctionApp.EventResultsClient(logger ?? NullLogger<FunctionApp.EventResultsClient>.Instance,
            options, webApiClient, converter);
    }
}
