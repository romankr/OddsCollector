using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute.ExceptionExtensions;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class BookmakerConverter
{
    [Test]
    public void ToOdds_WithNullBookmakers_ThrowsArgumentNullException()
    {
        var bookmakerConverter = CreateConverter(Substitute.For<FunctionApp.IMarketConverter>());

        var action = () => bookmakerConverter.ToOdds(null, AwayTeam, HomeTeam);

        action.Should().Throw<ArgumentNullException>().WithParameterName("bookmakers");
    }

    [TestCase("", TestName = "ToOdds_WithEmptyAwayTeam_ThrowsArgumentException")]
    [TestCase(null, TestName = "ToOdds_WithNullAwayTeam_ThrowsArgumentException")]
    [TestCase(" ", TestName = "ToOdds_WithWhitespaceAwayTeam_ThrowsArgumentException")]
    public void ToOdds_WithNullOrEmptyAwayTeam_ThrowsArgumentException(string? awayTeam)
    {
        var bookmakerConverter = CreateConverter(Substitute.For<FunctionApp.IMarketConverter>());

        var action = () => bookmakerConverter.ToOdds([], awayTeam, HomeTeam);

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("", TestName = "ToOdds_WithEmptyHomeTeam_ThrowsArgumentException")]
    [TestCase(null, TestName = "ToOdds_WithNullHomeTeam_ThrowsArgumentException")]
    [TestCase(" ", TestName = "ToOdds_WithWhitespaceHomeTeam_ThrowsArgumentException")]
    public void ToOdds_WithNullOrEmptyHomeTeam_ThrowsArgumentException(string? homeTeam)
    {
        var bookmakerConverter = CreateConverter(Substitute.For<FunctionApp.IMarketConverter>());

        var action = () => bookmakerConverter.ToOdds([], AwayTeam, homeTeam);

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
        using var scope = new AssertionScope();

        odds.Should().ContainSingle().Which.Bookmaker.Should().Be("working");

        var record = loggerMock.Collector.GetSnapshot().Should().ContainSingle().Subject;
        record.Level.Should().Be(LogLevel.Warning);
        record.Message.Should().Be($"Skipped bookmaker broken for {HomeTeam} - {AwayTeam}");
        record.Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Test]
    public void ToOdds_WithBookmakerMissingDrawPrice_SkipsItAndLogsWarning()
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
        using var scope = new AssertionScope();

        odds.Should().ContainSingle().Which.Bookmaker.Should().Be("working");
        loggerMock.Collector.GetSnapshot().Should().ContainSingle()
            .Which.Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Test]
    public void ToOdds_WithBookmakerMissingPrice_SkipsItAndLogsWarning()
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
        using var scope = new AssertionScope();

        odds.Should().ContainSingle().Which.Bookmaker.Should().Be("working");
        loggerMock.Collector.GetSnapshot().Should().ContainSingle()
            .Which.Exception.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().StartWith($"Outcome {HomeTeam} has no price");
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

        var bookmakerConverter = CreateConverter(marketConverterStub);

        // Act
        var action = () => bookmakerConverter.ToOdds([new Bookmakers { Key = "bookmaker" }], AwayTeam, HomeTeam)
            .ToList();

        // Assert
        action.Should().Throw<NotSupportedException>().Which.Should().BeSameAs(expectedException);
    }

    private const string HomeTeam = "homeTeam";
    private const string AwayTeam = "awayTeam";

    private static readonly Outcome HomeOutcome = new() { Name = HomeTeam, Price = 1.5 };
    private static readonly Outcome AwayOutcome = new() { Name = AwayTeam, Price = 4.0 };
    private static readonly Outcome DrawOutcome = new() { Name = OutcomeTypes.Draw, Price = 3.5 };

    private static FunctionApp.BookmakerConverter CreateConverter(FunctionApp.IMarketConverter marketConverter)
    {
        return new FunctionApp.BookmakerConverter(NullLogger<FunctionApp.BookmakerConverter>.Instance,
            marketConverter);
    }

    private static Bookmakers CreateBookmaker(string key, Markets2Key market, ICollection<Outcome> outcomes)
    {
        return new Bookmakers { Key = key, Markets = [new Markets2 { Key = market, Outcomes = outcomes }] };
    }
}
