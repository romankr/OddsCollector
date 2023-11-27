using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using OddsCollector.Common.KeyVault.Client;
using OddsCollector.Common.Models;
using OddsCollector.Common.OddsApi.Client;
using OddsCollector.Common.OddsApi.Configuration;
using OddsCollector.Common.OddsApi.Converter;
using OddsCollector.Common.OddsApi.WebApi;

namespace OddsCollector.Common.Tests.OddsApi.Client;

internal sealed class OddsApiClientTests
{
    [Test]
    public void Constructor_WithValidDependencies_ReturnsNewInstance()
    {
        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [] });
        var webApiClientStub = Substitute.For<IClient>();
        var keyVaultClientStub = Substitute.For<IKeyVaultClient>();
        var converterStub = Substitute.For<IOddsApiObjectConverter>();

        var result = new OddsApiClient(optionsStub, webApiClientStub, keyVaultClientStub, converterStub);

        result.Should().NotBeNull();
    }

    [Test]
    public void Constructor_WithNullOptions_ThrowsException()
    {
        var webApiClientStub = Substitute.For<IClient>();
        var keyVaultClientStub = Substitute.For<IKeyVaultClient>();
        var converterStub = Substitute.For<IOddsApiObjectConverter>();

        var action = () =>
        {
            _ = new OddsApiClient(null, webApiClientStub, keyVaultClientStub, converterStub);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Test]
    public void Constructor_WithNullLeaguesInOptions_ThrowsException()
    {
        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(null as OddsApiClientOptions);
        var webApiClientStub = Substitute.For<IClient>();
        var keyVaultClientStub = Substitute.For<IKeyVaultClient>();
        var converterStub = Substitute.For<IOddsApiObjectConverter>();

        var action = () =>
        {
            _ = new OddsApiClient(optionsStub, webApiClientStub, keyVaultClientStub, converterStub);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Test]
    public void Constructor_WithNullWebApiClient_ThrowsException()
    {
        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [] });
        var keyVaultClientStub = Substitute.For<IKeyVaultClient>();
        var converterStub = Substitute.For<IOddsApiObjectConverter>();

        var action = () =>
        {
            _ = new OddsApiClient(optionsStub, null, keyVaultClientStub, converterStub);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("webApiClient");
    }

    [Test]
    public void Constructor_WithNullKeyVaultClient_ThrowsException()
    {
        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [] });
        var webApiClientStub = Substitute.For<IClient>();
        var converterStub = Substitute.For<IOddsApiObjectConverter>();

        var action = () =>
        {
            _ = new OddsApiClient(optionsStub, webApiClientStub, null, converterStub);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("keyVaultClient");
    }

    [Test]
    public void Constructor_WithNullConverter_ThrowsException()
    {
        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [] });
        var webApiClientStub = Substitute.For<IClient>();
        var keyVaultClientStub = Substitute.For<IKeyVaultClient>();

        var action = () =>
        {
            _ = new OddsApiClient(optionsStub, webApiClientStub, keyVaultClientStub, null);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("objectConverter");
    }

    [Test]
    public async Task GetUpcomingEventsAsync_WithLeagues_ReturnsUpcomingEvents()
    {
        ICollection<Anonymous2> rawUpcomingEvents = [new()];
        var webApiClientMock = Substitute.For<IClient>();
        webApiClientMock
            .OddsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Regions>(), Arg.Any<Markets>(),
                Arg.Any<DateFormat>(), Arg.Any<OddsFormat>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.FromResult(rawUpcomingEvents));

        // ReSharper disable once CollectionNeverUpdated.Local
        List<UpcomingEvent> upcomingEvents = [];
        var converterMock = Substitute.For<IOddsApiObjectConverter>();
        converterMock.ToUpcomingEvents(Arg.Any<ICollection<Anonymous2>?>(), Arg.Any<Guid>(), Arg.Any<DateTime>())
            .Returns(new List<UpcomingEvent>());

        const string secretValue = nameof(secretValue);
        var keyVaultClientStub = Substitute.For<IKeyVaultClient>();
        keyVaultClientStub.GetOddsApiKey().Returns(Task.FromResult(secretValue));

        const string league = nameof(league);
        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [league] });

        var oddsClient = new OddsApiClient(optionsStub, webApiClientMock, keyVaultClientStub, converterMock);

        var traceId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var results = await oddsClient.GetUpcomingEventsAsync(traceId, timestamp);

        results.Should().NotBeNull().And.Equal(upcomingEvents);

        await webApiClientMock.Received()
            .OddsAsync(league, secretValue, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal, null, null)
            .ConfigureAwait(false);

        var received = converterMock.ReceivedCalls().ToList();

        received.Should().NotBeNull();
        received.Should().HaveCount(1);

        var firstReceived = received.First();

        firstReceived.GetMethodInfo().Name.Should().Be("ToUpcomingEvents");

        var firstReceivedArguments = firstReceived.GetArguments();

        firstReceivedArguments.Should().NotBeNull();
        firstReceivedArguments[0].Should().Be(rawUpcomingEvents);
        firstReceivedArguments[1].Should().Be(traceId);
        firstReceivedArguments[2].Should().Be(timestamp);
    }

    [Test]
    public async Task GetEventResultsAsync_WithLeagues_ReturnsEventResults()
    {
        ICollection<Anonymous3> rawEventResults = [new()];
        var webApiClientMock = Substitute.For<IClient>();
        webApiClientMock.ScoresAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>())
            .Returns(Task.FromResult(rawEventResults));

        // ReSharper disable once CollectionNeverUpdated.Local
        List<EventResult> eventResults = [];
        var converterMock = Substitute.For<IOddsApiObjectConverter>();
        converterMock.ToEventResults(Arg.Any<ICollection<Anonymous3>?>(), Arg.Any<Guid>(), Arg.Any<DateTime>())
            .Returns(eventResults);

        const string league = nameof(league);
        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [league] });

        const string secretValue = nameof(secretValue);
        var keyVaultClientStub = Substitute.For<IKeyVaultClient>();
        keyVaultClientStub.GetOddsApiKey().Returns(Task.FromResult(secretValue));

        var oddsClient = new OddsApiClient(optionsStub, webApiClientMock, keyVaultClientStub, converterMock);

        var traceId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var results = await oddsClient.GetEventResultsAsync(traceId, timestamp);

        results.Should().NotBeNull().And.Equal(eventResults);

        await webApiClientMock.Received().ScoresAsync(league, secretValue, 3).ConfigureAwait(false);

        var received = converterMock.ReceivedCalls().ToList();

        received.Should().NotBeNull().And.HaveCount(1);

        var firstReceived = received.First();

        firstReceived.GetMethodInfo().Name.Should().Be("ToEventResults");

        var firstReceivedArguments = firstReceived.GetArguments();

        firstReceivedArguments.Should().NotBeNull();
        firstReceivedArguments[0].Should().Be(rawEventResults);
        firstReceivedArguments[1].Should().Be(traceId);
        firstReceivedArguments[2].Should().Be(timestamp);
    }
}
