using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class BookmakerConverter
{
    [Test]
    public void ToOdds_WithNullBookmakers_ThrowsException()
    {
        var marketConverter = Substitute.For<FunctionApp.IMarketConverter>();

        var bookmakerConverter = new FunctionApp.BookmakerConverter(
            NullLogger<FunctionApp.BookmakerConverter>.Instance, marketConverter);

        var action = () => bookmakerConverter.ToOdds(null, "awayTeam", "homeTeam");

        action.Should().Throw<ArgumentNullException>().WithParameterName("bookmakers");
    }

    [TestCase("", TestName = "ToOdds_WithEmptyAwayTeam_ThrowsException")]
    [TestCase(null, TestName = "ToOdds_WithNullAwayTeam_ThrowsException")]
    [TestCase(" ", TestName = "ToOdds_WithWhitespaceAwayTeam_ThrowsException")]
    public void ToOdds_WithNullOrEmptyAwayTeam_ThrowsException(string? awayTeam)
    {
        var marketConverter = Substitute.For<FunctionApp.IMarketConverter>();

        var bookmakerConverter = new FunctionApp.BookmakerConverter(
            NullLogger<FunctionApp.BookmakerConverter>.Instance, marketConverter);

        var action = () => bookmakerConverter.ToOdds([], awayTeam, "homeTeam");

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("", TestName = "ToOdds_WithEmptyHomeTeam_ThrowsException")]
    [TestCase(null, TestName = "ToOdds_WithNullHomeTeam_ThrowsException")]
    [TestCase(" ", TestName = "ToOdds_WithWhitespaceHomeTeam_ThrowsException")]
    public void ToOdds_WithNullOrEmptyHomeTeam_ThrowsException(string? homeTeam)
    {
        var marketConverter = Substitute.For<FunctionApp.IMarketConverter>();

        var bookmakerConverter = new FunctionApp.BookmakerConverter(
            NullLogger<FunctionApp.BookmakerConverter>.Instance, marketConverter);

        var action = () => bookmakerConverter.ToOdds([], "awayTeam", homeTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [Test]
    public void ToOdds_WithBookmakerMissingHeadToHeadMarket_SkipsItAndLogsWarning()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.BookmakerConverter>();

        var bookmakerConverter = new FunctionApp.BookmakerConverter(loggerMock,
            new FunctionApp.MarketConverter(new FunctionApp.OddConverter()));

        Bookmakers[] bookmakers =
        [
            CreateBookmaker("broken", Markets2Key.Spreads, [HomeOutcome, AwayOutcome, DrawOutcome]),
            CreateBookmaker("working", Markets2Key.H2h, [HomeOutcome, AwayOutcome, DrawOutcome])
        ];

        // Act
        var odds = bookmakerConverter.ToOdds(bookmakers, AwayTeam, HomeTeam).ToList();

        // Assert
        odds.Should().ContainSingle().Which.Bookmaker.Should().Be("working");

        loggerMock.Collector.Count.Should().Be(1);
        loggerMock.LatestRecord.Level.Should().Be(LogLevel.Warning);
        loggerMock.LatestRecord.Message.Should().Be($"Skipped bookmaker broken for {HomeTeam} - {AwayTeam}");
        loggerMock.LatestRecord.Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Test]
    public void ToOdds_WithBookmakerMissingDrawPrice_SkipsIt()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.BookmakerConverter>();

        var bookmakerConverter = new FunctionApp.BookmakerConverter(loggerMock,
            new FunctionApp.MarketConverter(new FunctionApp.OddConverter()));

        Bookmakers[] bookmakers =
        [
            CreateBookmaker("working", Markets2Key.H2h, [HomeOutcome, AwayOutcome, DrawOutcome]),
            CreateBookmaker("broken", Markets2Key.H2h, [HomeOutcome, AwayOutcome])
        ];

        // Act
        var odds = bookmakerConverter.ToOdds(bookmakers, AwayTeam, HomeTeam).ToList();

        // Assert
        odds.Should().ContainSingle().Which.Bookmaker.Should().Be("working");
        loggerMock.Collector.Count.Should().Be(1);
    }

    [Test]
    public void ToOdds_WithBookmakerMissingPrice_SkipsIt()
    {
        // Arrange
        var loggerMock = new FakeLogger<FunctionApp.BookmakerConverter>();

        var bookmakerConverter = new FunctionApp.BookmakerConverter(loggerMock,
            new FunctionApp.MarketConverter(new FunctionApp.OddConverter()));

        Bookmakers[] bookmakers =
        [
            CreateBookmaker("broken", Markets2Key.H2h,
                [new Outcome { Name = HomeTeam, Price = null }, AwayOutcome, DrawOutcome]),
            CreateBookmaker("working", Markets2Key.H2h, [HomeOutcome, AwayOutcome, DrawOutcome])
        ];

        // Act
        var odds = bookmakerConverter.ToOdds(bookmakers, AwayTeam, HomeTeam).ToList();

        // Assert
        odds.Should().ContainSingle().Which.Bookmaker.Should().Be("working");
        loggerMock.LatestRecord.Exception.Should().BeOfType<ArgumentNullException>();
    }

    [Test]
    public void ToOdds_WithUnexpectedException_LetsItThrough()
    {
        // Arrange
        var expectedException = new NotSupportedException();

        var marketConverterStub = Substitute.For<FunctionApp.IMarketConverter>();
        marketConverterStub
            .ToOdd(Arg.Any<ICollection<Markets2>?>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>())
            .Throws(expectedException);

        var bookmakerConverter = new FunctionApp.BookmakerConverter(
            NullLogger<FunctionApp.BookmakerConverter>.Instance, marketConverterStub);

        // Act
        var action = () => bookmakerConverter.ToOdds([new Bookmakers { Key = "bookmaker" }], AwayTeam, HomeTeam)
            .ToList();

        // Assert: only a malformed bookmaker is skipped; anything else is a fault in the run.
        action.Should().Throw<NotSupportedException>().Which.Should().BeSameAs(expectedException);
    }

    private const string HomeTeam = "homeTeam";
    private const string AwayTeam = "awayTeam";

    private static readonly Outcome HomeOutcome = new() { Name = HomeTeam, Price = 1.5 };
    private static readonly Outcome AwayOutcome = new() { Name = AwayTeam, Price = 4.0 };
    private static readonly Outcome DrawOutcome = new() { Name = "Draw", Price = 3.5 };

    private static Bookmakers CreateBookmaker(string key, Markets2Key market, ICollection<Outcome> outcomes)
    {
        return new Bookmakers { Key = key, Markets = [new Markets2 { Key = market, Outcomes = outcomes }] };
    }
}
