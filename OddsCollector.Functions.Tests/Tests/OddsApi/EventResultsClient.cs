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

internal sealed class EventResultsClient
{
    private const string SecretValue = nameof(SecretValue);

    [Test]
    public async Task GetEventResultsAsync_WithLeagues_ReturnsEventResults()
    {
        // Arrange
        const string league = nameof(league);

        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [league], ApiKey = SecretValue });

        ICollection<Anonymous3> rawItems = [new()];
        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub.ScoresAsync(league, SecretValue, 3, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(rawItems));

        EventResult[] converted = [new()];
        var converterStub = Substitute.For<IOriginalCompletedEventConverter>();
        converterStub.ToEventResults(rawItems).Returns(converted);

        var loggerStub = new FakeLogger<FunctionApp.EventResultsClient>();

        var oddsClient =
            new FunctionApp.EventResultsClient(loggerStub, optionsStub, webApiClientStub, converterStub);

        // Act
        var results = await oddsClient.GetEventResultsAsync(CancellationToken.None);

        // Assert
        results.Should().NotBeNull().And.HaveCount(1).And.Equal(converted);
    }

    [Test]
    public async Task GetEventResultsAsync_WithLeaguesAndRequestedCancellation_ReturnsNoEventResults()
    {
        // Arrange
        const string league = nameof(league);

        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [league], ApiKey = SecretValue });

        ICollection<Anonymous3> rawItems = [new()];
        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub.ScoresAsync(league, SecretValue, 3, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(rawItems));

        EventResult[] converted = [new()];
        var converterStub = Substitute.For<IOriginalCompletedEventConverter>();
        converterStub.ToEventResults(rawItems).Returns(converted);

        var loggerStub = new FakeLogger<FunctionApp.EventResultsClient>();

        var oddsClient =
            new FunctionApp.EventResultsClient(loggerStub, optionsStub, webApiClientStub, converterStub);

        var cancellationToken = await CancellationTokenGenerator.GetRequestedForCancellationToken();

        // Act
        var results = await oddsClient.GetEventResultsAsync(cancellationToken);

        // Assert
        results.Should().NotBeNull().And.HaveCount(0);
    }

    [Test]
    public async Task GetEventResultsAsync_WithFailingLeague_SkipsItAndLogsError()
    {
        // Arrange
        const string failingLeague = nameof(failingLeague);
        const string workingLeague = nameof(workingLeague);

        var expectedException = new Exception();

        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions
        {
            Leagues = [failingLeague, workingLeague],
            ApiKey = SecretValue
        });

        ICollection<Anonymous3> rawItems = [new()];
        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub.ScoresAsync(failingLeague, SecretValue, 3, Arg.Any<CancellationToken>())
            .Throws(expectedException);
        webApiClientStub.ScoresAsync(workingLeague, SecretValue, 3, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(rawItems));

        EventResult[] converted = [new()];
        var converterStub = Substitute.For<IOriginalCompletedEventConverter>();
        converterStub.ToEventResults(rawItems).Returns(converted);

        var loggerMock = new FakeLogger<FunctionApp.EventResultsClient>();

        var oddsClient =
            new FunctionApp.EventResultsClient(loggerMock, optionsStub, webApiClientStub, converterStub);

        // Act
        var results = await oddsClient.GetEventResultsAsync(CancellationToken.None);

        // Assert
        results.Should().NotBeNull().And.HaveCount(1).And.Equal(converted);

        loggerMock.Collector.Count.Should().Be(1);

        using var scope = new AssertionScope();

        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Error);
        loggerMock.LatestRecord.Message.Should().Be($"Failed to get event results for {failingLeague}");
        loggerMock.LatestRecord.Exception.Should().Be(expectedException);
    }
}
