using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converter;
using OddsCollector.Functions.OddsApi.WebApi;
using OddsCollector.Functions.Tests.Infrastructure.CancellationToken;

namespace OddsCollector.Functions.Tests.Tests.OddsApi;

internal class OddsApiClient
{
    [Test]
    public async Task GetUpcomingEventsAsync_WithLeagues_ReturnsUpcomingEvents()
    {
        // Arrange
        ICollection<Anonymous2> rawUpcomingEvents = [new Anonymous2()];
        var webApiClientMock = Substitute.For<IClient>();
        webApiClientMock
            .OddsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Regions>(), Arg.Any<Markets>(),
                Arg.Any<DateFormat>(), Arg.Any<OddsFormat>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(rawUpcomingEvents));

        // ReSharper disable once CollectionNeverUpdated.Local
        List<UpcomingEvent> upcomingEvents = [];
        var converterMock = Substitute.For<IOddsApiObjectConverter>();
        converterMock.ToUpcomingEvents(Arg.Any<ICollection<Anonymous2>?>()).Returns(new List<UpcomingEvent>());

        const string secretValue = nameof(secretValue);

        const string league = nameof(league);
        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [league], ApiKey = secretValue });

        var oddsClient =
            new OddsCollector.Functions.OddsApi.OddsApiClient(optionsStub, webApiClientMock, converterMock);

        var token = new CancellationToken();

        // Act
        var results = await oddsClient.GetUpcomingEventsAsync(token);

        // Assert
        results.Should().NotBeNull().And.Equal(upcomingEvents);

        await webApiClientMock.Received()
            .OddsAsync(league, secretValue, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal, null, null,
                token)
            .ConfigureAwait(false);

        var received = converterMock.ReceivedCalls().ToList();

        received.Should().NotBeNull().And.HaveCount(1);

        var firstReceived = received.First();

        firstReceived.GetMethodInfo().Name.Should().Be("ToUpcomingEvents");

        var firstReceivedArguments = firstReceived.GetArguments();

        firstReceivedArguments.Should().NotBeNull();
        firstReceivedArguments[0].Should().Be(rawUpcomingEvents);
    }

    [Test]
    public async Task GetEventResultsAsync_WithLeagues_ReturnsEventResults()
    {
        // Arrange
        ICollection<Anonymous3> rawEventResults = [new Anonymous3()];
        var webApiClientMock = Substitute.For<IClient>();
        webApiClientMock.ScoresAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(rawEventResults));

        // ReSharper disable once CollectionNeverUpdated.Local
        List<EventResult> eventResults = [];
        var converterMock = Substitute.For<IOddsApiObjectConverter>();
        converterMock.ToEventResults(Arg.Any<ICollection<Anonymous3>?>()).Returns(eventResults);

        const string secretValue = nameof(secretValue);

        const string league = nameof(league);
        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [league], ApiKey = secretValue });

        var oddsClient =
            new OddsCollector.Functions.OddsApi.OddsApiClient(optionsStub, webApiClientMock, converterMock);

        var token = new CancellationToken();

        // Act
        var results = await oddsClient.GetEventResultsAsync(token);

        // Assert
        results.Should().NotBeNull().And.Equal(eventResults);

        await webApiClientMock.Received().ScoresAsync(league, secretValue, 3, token).ConfigureAwait(false);

        var received = converterMock.ReceivedCalls().ToList();

        received.Should().NotBeNull().And.HaveCount(1);

        var firstReceived = received.First();

        firstReceived.GetMethodInfo().Name.Should().Be("ToEventResults");

        var firstReceivedArguments = firstReceived.GetArguments();

        firstReceivedArguments.Should().NotBeNull();
        firstReceivedArguments[0].Should().Be(rawEventResults);
    }

    [Test]
    public async Task GetUpcomingEventsAsync_WithLeaguesAndRequestedCancellation_ReturnsNoUpcomingEvents()
    {
        // Arrange
        ICollection<Anonymous2> rawUpcomingEvents = [new Anonymous2()];
        var webApiClientMock = Substitute.For<IClient>();
        webApiClientMock
            .OddsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Regions>(), Arg.Any<Markets>(),
                Arg.Any<DateFormat>(), Arg.Any<OddsFormat>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(rawUpcomingEvents));

        // ReSharper disable once CollectionNeverUpdated.Local
        List<UpcomingEvent> upcomingEvents = [];
        var converterMock = Substitute.For<IOddsApiObjectConverter>();
        converterMock.ToUpcomingEvents(Arg.Any<ICollection<Anonymous2>?>()).Returns(new List<UpcomingEvent>());

        const string secretValue = nameof(secretValue);

        const string league = nameof(league);
        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [league], ApiKey = secretValue });

        var oddsClient =
            new OddsCollector.Functions.OddsApi.OddsApiClient(optionsStub, webApiClientMock, converterMock);

        var cancellationToken = await CancellationTokenGenerator.GetRequestedForCancellationToken();

        // Act
        var results = (await oddsClient.GetUpcomingEventsAsync(cancellationToken)).ToList();

        // Assert
        results.Should().NotBeNull().And.HaveCount(0);
    }

    [Test]
    public async Task GetEventResultsAsync_WithLeaguesAndRequestedCancellation_ReturnsNoEventResults()
    {
        // Arrange
        ICollection<Anonymous3> rawEventResults = [new Anonymous3()];
        var webApiClientMock = Substitute.For<IClient>();
        webApiClientMock.ScoresAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(rawEventResults));

        // ReSharper disable once CollectionNeverUpdated.Local
        List<EventResult> eventResults = [];
        var converterMock = Substitute.For<IOddsApiObjectConverter>();
        converterMock.ToEventResults(Arg.Any<ICollection<Anonymous3>?>()).Returns(eventResults);

        const string secretValue = nameof(secretValue);

        const string league = nameof(league);
        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [league], ApiKey = secretValue });

        var oddsClient =
            new OddsCollector.Functions.OddsApi.OddsApiClient(optionsStub, webApiClientMock, converterMock);

        var cancellationToken = await CancellationTokenGenerator.GetRequestedForCancellationToken();

        // Act
        var results = (await oddsClient.GetEventResultsAsync(cancellationToken)).ToList();

        // Assert
        results.Should().NotBeNull().And.HaveCount(0);
    }
}
