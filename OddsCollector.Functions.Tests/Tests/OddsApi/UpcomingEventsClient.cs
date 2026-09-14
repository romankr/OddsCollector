using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;
using OddsCollector.Functions.Tests.Infrastructure.CancellationToken;
using FunctionApp = OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.OddsApi;

internal sealed class UpcomingEventsClient
{
    private const string SecretValue = nameof(SecretValue);

    [Test]
    public async Task GetUpcomingEventsAsync_WithLeagues_ReturnsUpcomingEvents()
    {
        // Arrange
        const string league = nameof(league);

        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [league], ApiKey = SecretValue });

        ICollection<Anonymous2> rawItems = [new()];
        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub
            .OddsAsync(league, SecretValue, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal, null, null,
                Arg.Any<CancellationToken>()).Returns(Task.FromResult(rawItems));

        UpcomingEvent[] converted = [new()];
        var converterStub = Substitute.For<IOriginalUpcomingEventConverter>();
        converterStub.ToUpcomingEvents(rawItems).Returns(converted);

        var loggerStub = new FakeLogger<FunctionApp.UpcomingEventsClient>();

        var oddsClient =
            new FunctionApp.UpcomingEventsClient(loggerStub, optionsStub, webApiClientStub, converterStub);

        // Act
        var results = await oddsClient.GetUpcomingEventsAsync(CancellationToken.None);

        // Assert
        results.Should().NotBeNull().And.HaveCount(1).And.Equal(converted);
    }

    [Test]
    public async Task GetUpcomingEventsAsync_WithLeaguesAndRequestedCancellation_ReturnsNoUpcomingEvents()
    {
        // Arrange
        const string league = nameof(league);

        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [league], ApiKey = SecretValue });

        ICollection<Anonymous2> rawItems = [new()];
        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub
            .OddsAsync(league, SecretValue, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal, null, null,
                Arg.Any<CancellationToken>()).Returns(Task.FromResult(rawItems));

        UpcomingEvent[] converted = [new()];
        var converterStub = Substitute.For<IOriginalUpcomingEventConverter>();
        converterStub.ToUpcomingEvents(rawItems).Returns(converted);

        var loggerStub = new FakeLogger<FunctionApp.UpcomingEventsClient>();

        var oddsClient =
            new FunctionApp.UpcomingEventsClient(loggerStub, optionsStub, webApiClientStub, converterStub);

        var cancellationToken = await CancellationTokenGenerator.GetRequestedForCancellationToken();

        // Act
        var results = await oddsClient.GetUpcomingEventsAsync(cancellationToken);

        // Assert
        results.Should().NotBeNull().And.HaveCount(0);
    }

    [Test]
    public async Task GetUpcomingEventsAsync_WithFailingLeague_SkipsItAndLogsError()
    {
        // Arrange
        const string failingLeague = nameof(failingLeague);
        const string workingLeague = nameof(workingLeague);

        var expectedException = new Exception();

        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions
        {
            Leagues = [failingLeague, workingLeague], ApiKey = SecretValue
        });

        ICollection<Anonymous2> rawItems = [new()];
        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub
            .OddsAsync(failingLeague, SecretValue, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal, null,
                null, Arg.Any<CancellationToken>()).Throws(expectedException);
        webApiClientStub
            .OddsAsync(workingLeague, SecretValue, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal, null,
                null, Arg.Any<CancellationToken>()).Returns(Task.FromResult(rawItems));

        UpcomingEvent[] converted = [new()];
        var converterStub = Substitute.For<IOriginalUpcomingEventConverter>();
        converterStub.ToUpcomingEvents(rawItems).Returns(converted);

        var loggerMock = new FakeLogger<FunctionApp.UpcomingEventsClient>();

        var oddsClient =
            new FunctionApp.UpcomingEventsClient(loggerMock, optionsStub, webApiClientStub, converterStub);

        // Act
        var results = await oddsClient.GetUpcomingEventsAsync(CancellationToken.None);

        // Assert
        results.Should().NotBeNull().And.HaveCount(1).And.Equal(converted);

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Error);
        loggerMock.LatestRecord.Message.Should().Be($"Failed to get upcoming events for {failingLeague}");
        loggerMock.LatestRecord.Exception.Should().Be(expectedException);
    }
}
