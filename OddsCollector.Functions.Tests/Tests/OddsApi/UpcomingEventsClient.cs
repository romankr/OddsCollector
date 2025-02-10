using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Configuration;
using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;
using OddsCollector.Functions.Tests.Infrastructure.CancellationToken;
using FunctionApp = OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.OddsApi;

internal sealed class UpcomingEventsClient
{
    [Test]
    public async Task GetUpcomingEventsAsync_WithLeagues_ReturnsUpcomingEvents()
    {
        // Arrange
        const string secretValue = nameof(secretValue);

        const string league = nameof(league);

        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [league], ApiKey = secretValue });

        ICollection<Anonymous2> rawUpcomingEvents = [new()];
        var webApiClientMock = Substitute.For<IClient>();
        webApiClientMock
            .OddsAsync(league, secretValue, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal, null, null,
                Arg.Any<CancellationToken>()).Returns(Task.FromResult(rawUpcomingEvents));

        UpcomingEvent[] upcomingEvents = [new()];
        var converterMock = Substitute.For<IOriginalUpcomingEventConverter>();
        converterMock.ToUpcomingEvents(rawUpcomingEvents).Returns(upcomingEvents);

        var oddsClient = new FunctionApp.UpcomingEventsClient(optionsStub, webApiClientMock, converterMock);

        // Act
        var results = await oddsClient.GetUpcomingEventsAsync(CancellationToken.None);

        // Assert
        results.Should().NotBeNull().And.HaveCount(1).And.Equal(upcomingEvents);
    }

    [Test]
    public async Task GetUpcomingEventsAsync_WithLeaguesAndRequestedCancellation_ReturnsNoUpcomingEvents()
    {
        // Arrange
        const string secretValue = nameof(secretValue);

        const string league = nameof(league);

        var optionsStub = Substitute.For<IOptions<OddsApiClientOptions>>();
        optionsStub.Value.Returns(new OddsApiClientOptions { Leagues = [league], ApiKey = secretValue });

        ICollection<Anonymous2> rawUpcomingEvents = [new()];
        var webApiClientMock = Substitute.For<IClient>();
        webApiClientMock
            .OddsAsync(league, secretValue, Regions.Eu, Markets.H2h, DateFormat.Iso, OddsFormat.Decimal, null, null,
                Arg.Any<CancellationToken>()).Returns(Task.FromResult(rawUpcomingEvents));

        UpcomingEvent[] upcomingEvents = [new()];
        var converterMock = Substitute.For<IOriginalUpcomingEventConverter>();
        converterMock.ToUpcomingEvents(rawUpcomingEvents).Returns(upcomingEvents);

        var oddsClient = new FunctionApp.UpcomingEventsClient(optionsStub, webApiClientMock, converterMock);

        var cancellationToken = await CancellationTokenGenerator.GetRequestedForCancellationToken();

        // Act
        var results = await oddsClient.GetUpcomingEventsAsync(cancellationToken);

        // Assert
        results.Should().NotBeNull().And.HaveCount(0);
    }
}
