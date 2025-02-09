using Microsoft.Extensions.Options;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;
using OddsCollector.Functions.Tests.Infrastructure.CancellationToken;
using FunctionApp = OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.OddsApi;

internal sealed class EventResultsClient
{
    [Test]
    public async Task GetEventResultsAsync_WithLeagues_ReturnsEventResults()
    {
        // Arrange
        const string secretValue = nameof(secretValue);

        const string league = nameof(league);

        var optionsStub = Substitute.For<IOptions<FunctionApp.Configuration.OddsApiClientOptions>>();
        optionsStub.Value.Returns(new FunctionApp.Configuration.OddsApiClientOptions
        {
            Leagues = [league],
            ApiKey = secretValue
        });

        ICollection<Anonymous3>? rawEventResults = [new Anonymous3()];
        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub.ScoresAsync(league, secretValue, 3, Arg.Any<CancellationToken>()).Returns(Task.FromResult(rawEventResults));

        EventResult[] eventResults = [new EventResult()];
        var converterStub = Substitute.For<FunctionApp.Converters.IOriginalCompletedEventConverter>();
        converterStub.ToEventResults(rawEventResults).Returns(eventResults);

        var oddsClient = new FunctionApp.EventResultsClient(optionsStub, webApiClientStub, converterStub);

        // Act
        var results = await oddsClient.GetEventResultsAsync(new CancellationToken());

        // Assert
        results.Should().NotBeNull().And.HaveCount(1).And.Equal(eventResults);
    }

    [Test]
    public async Task GetEventResultsAsync_WithLeaguesAndRequestedCancellation_ReturnsNoEventResults()
    {
        // Arrange
        const string secretValue = nameof(secretValue);

        const string league = nameof(league);

        var optionsStub = Substitute.For<IOptions<FunctionApp.Configuration.OddsApiClientOptions>>();
        optionsStub.Value.Returns(new FunctionApp.Configuration.OddsApiClientOptions
        {
            Leagues = [league],
            ApiKey = secretValue
        });

        ICollection<Anonymous3>? rawEventResults = [new Anonymous3()];
        var webApiClientStub = Substitute.For<IClient>();
        webApiClientStub.ScoresAsync(league, secretValue, 3, Arg.Any<CancellationToken>()).Returns(Task.FromResult(rawEventResults));

        EventResult[] eventResults = [new EventResult()];
        var converterStub = Substitute.For<FunctionApp.Converters.IOriginalCompletedEventConverter>();
        converterStub.ToEventResults(rawEventResults).Returns(eventResults);

        var oddsClient = new FunctionApp.EventResultsClient(optionsStub, webApiClientStub, converterStub);

        var cancellationToken = await CancellationTokenGenerator.GetRequestedForCancellationToken();

        // Act
        var results = await oddsClient.GetEventResultsAsync(cancellationToken);

        // Assert
        results.Should().NotBeNull().And.HaveCount(0);
    }
}
