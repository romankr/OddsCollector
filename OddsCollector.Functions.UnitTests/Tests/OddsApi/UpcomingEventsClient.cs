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
using OddsCollector.Functions.Tests.Infrastructure.Models;
using FunctionApp = OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.OddsApi;

internal sealed class UpcomingEventsClient
{
    private const string ApiKey = "apiKey";
    private const string League = "league";


    [Test]
    public async Task GetUpcomingEventsAsync_WithLeague_ReturnsConvertedUpcomingEvents()
    {
        // Arrange
        ICollection<Anonymous2> originalEvents = [new()];
        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub.OddsAsync(League, ApiKey, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal, null,
            null, Arg.Any<CancellationToken>()).Returns(originalEvents);

        UpcomingEvent[] upcomingEvents = [ValidModels.CreateUpcomingEvent()];
        var converterStub = Substitute.For<IOriginalUpcomingEventConverter>();
        converterStub.ToUpcomingEvents(originalEvents).Returns(upcomingEvents);

        var client = CreateClient([League], webApiClientStub, converterStub);

        // Act
        var results = await client.GetUpcomingEventsAsync(CancellationToken.None);

        // Assert
        results.Should().Equal(upcomingEvents);
    }

    [Test]
    public async Task GetUpcomingEventsAsync_WithCancellationBeforeFirstLeague_ReturnsNothingWithoutCallingApi()
    {
        // Arrange
        var webApiClientMock = Substitute.For<IClient>();

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsClient>();

        var client = CreateClient([League], webApiClientMock, Substitute.For<IOriginalUpcomingEventConverter>(), loggerMock);

        // Act
        var results = await client.GetUpcomingEventsAsync(new CancellationToken(canceled: true));

        // Assert
        using var scope = new AssertionScope();

        results.Should().BeEmpty();
        webApiClientMock.ReceivedCalls().Should().BeEmpty();
        loggerMock.Collector.GetSnapshot().Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Level = LogLevel.Information,
            Message = "Collection was cancelled, keeping 0 upcoming events collected so far"
        });
    }

    [Test]
    public async Task GetUpcomingEventsAsync_WithCancellationDuringLeague_KeepsResultsOfEarlierLeagues()
    {
        // Arrange
        const string answeredLeague = nameof(answeredLeague);
        const string cancelledLeague = nameof(cancelledLeague);
        const string skippedLeague = nameof(skippedLeague);

        using var cancellation = new CancellationTokenSource();

        ICollection<Anonymous2> originalEvents = [new()];
        var webApiClientMock = Substitute.For<IClient>();
        webApiClientMock.OddsAsync(answeredLeague, ApiKey, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal,
            null, null, Arg.Any<CancellationToken>())
            .Returns(originalEvents);
        webApiClientMock.OddsAsync(cancelledLeague, ApiKey, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal,
            null, null, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                // The host cancels the run while the request is in flight.
                cancellation.Cancel();
                return Task.FromCanceled<ICollection<Anonymous2>>(cancellation.Token);
            });

        UpcomingEvent[] converted = [ValidModels.CreateUpcomingEvent()];
        var converterStub = Substitute.For<IOriginalUpcomingEventConverter>();
        converterStub.ToUpcomingEvents(originalEvents).Returns(converted);

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsClient>();

        var client = CreateClient([answeredLeague, cancelledLeague, skippedLeague], webApiClientMock,
            converterStub, loggerMock);

        // Act
        var results = await client.GetUpcomingEventsAsync(cancellation.Token);

        // Assert
        using var scope = new AssertionScope();

        results.Should().Equal(converted);
        await webApiClientMock.DidNotReceive().OddsAsync(skippedLeague, ApiKey, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal,
            null, null, Arg.Any<CancellationToken>());
        loggerMock.Collector.GetSnapshot().Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            Level = LogLevel.Information,
            Message = "Collection was cancelled, keeping 1 upcoming events collected so far"
        });
    }

    [Test]
    public async Task GetUpcomingEventsAsync_WithFailingLeague_SkipsItAndLogsError()
    {
        // Arrange
        const string failingLeague = nameof(failingLeague);
        const string workingLeague = nameof(workingLeague);

        var expectedException = new HttpRequestException();

        ICollection<Anonymous2> originalEvents = [new()];
        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub.OddsAsync(failingLeague, ApiKey, Regions.Eu, Markets.H2h, DateFormat.Iso,
            OddsFormat.Decimal, null, null, Arg.Any<CancellationToken>()).Throws(expectedException);
        webApiClientStub.OddsAsync(workingLeague, ApiKey, Regions.Eu, Markets.H2h, DateFormat.Iso,
            OddsFormat.Decimal, null, null, Arg.Any<CancellationToken>()).Returns(originalEvents);

        UpcomingEvent[] upcomingEvents = [ValidModels.CreateUpcomingEvent()];
        var converterStub = Substitute.For<IOriginalUpcomingEventConverter>();
        converterStub.ToUpcomingEvents(originalEvents).Returns(upcomingEvents);

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsClient>();

        var client = CreateClient([failingLeague, workingLeague], webApiClientStub, converterStub, loggerMock);

        // Act
        var results = await client.GetUpcomingEventsAsync(CancellationToken.None);

        // Assert
        using var scope = new AssertionScope();

        results.Should().Equal(upcomingEvents);

        var record = loggerMock.Collector.GetSnapshot().Should().ContainSingle().Subject;
        record.Level.Should().Be(LogLevel.Error);
        record.Message.Should().Be($"Failed to get upcoming events for {failingLeague}");
        record.Exception.Should().BeSameAs(expectedException);
    }

    [Test]
    public async Task GetUpcomingEventsAsync_WithEveryLeagueFailing_ThrowsWithEveryFailure()
    {
        // Arrange
        const string firstLeague = nameof(firstLeague);
        const string secondLeague = nameof(secondLeague);

        var firstException = new HttpRequestException();
        var secondException = new HttpRequestException();

        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub.OddsAsync(firstLeague, ApiKey, Regions.Eu, Markets.H2h, DateFormat.Iso,
            OddsFormat.Decimal, null, null, Arg.Any<CancellationToken>()).Throws(firstException);
        webApiClientStub.OddsAsync(secondLeague, ApiKey, Regions.Eu, Markets.H2h, DateFormat.Iso,
            OddsFormat.Decimal, null, null, Arg.Any<CancellationToken>()).Throws(secondException);

        var client = CreateClient([firstLeague, secondLeague], webApiClientStub,
            Substitute.For<IOriginalUpcomingEventConverter>());

        // Act
        var action = () => client.GetUpcomingEventsAsync(CancellationToken.None);

        // Assert
        var exception = (await action.Should().ThrowExactlyAsync<AggregateException>()).Which;
        exception.InnerExceptions.Should().Equal(firstException, secondException);
        exception.Message.Should().StartWith("Failed to get upcoming events for every league");
    }

    private static FunctionApp.UpcomingEventsClient CreateClient(HashSet<string> leagues, IClient webApiClient,
        IOriginalUpcomingEventConverter converter, ILogger<FunctionApp.UpcomingEventsClient>? logger = null)
    {
        var options = Options.Create(new OddsApiClientOptions { Leagues = leagues, ApiKey = ApiKey });

        return new FunctionApp.UpcomingEventsClient(logger ?? NullLogger<FunctionApp.UpcomingEventsClient>.Instance,
            options, webApiClient, converter);
    }
}
