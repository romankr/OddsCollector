using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Converter;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.Tests.OddsApi.Converter;

[Parallelizable(ParallelScope.All)]
internal sealed class OddsApiObjectConverterTests
{
    [Test]
    public void ToUpcomingEvents_WithOddList_ReturnsConvertedEvents()
    {
        var converter = new OddsApiObjectConverter();

        var timestamp = DateTime.UtcNow;
        var traceId = Guid.NewGuid();

        List<Anonymous2> rawUpcomingEvents =
        [
            new TestAnonymous2Builder().SetDefaults().Instance,
            new TestAnonymous2Builder().SetDefaults().SetId("1766194919f1cbfbd846576434f0499b").Instance
        ];

        var upcomingEvents = converter.ToUpcomingEvents(rawUpcomingEvents, traceId, timestamp).ToList();

        upcomingEvents.Should().NotBeNull().And.HaveCount(2);

        var firstEvent = upcomingEvents.ElementAt(0);

        firstEvent.Should().NotBeNull();
        firstEvent.AwayTeam.Should().NotBeNull().And.Be(TestAnonymous2Builder.DefaultAwayTeam);
        firstEvent.CommenceTime.Should().Be(TestAnonymous2Builder.DefaultCommenceTime);
        firstEvent.HomeTeam.Should().NotBeNull().And.Be(TestAnonymous2Builder.DefaultHomeTeam);
        firstEvent.Id.Should().NotBeNull().And.Be(TestAnonymous2Builder.DefaultId);
        firstEvent.Timestamp.Should().Be(timestamp);
        firstEvent.TraceId.Should().Be(traceId);
        firstEvent.Odds.Should().NotBeNull().And.HaveCount(2);
        firstEvent.Odds.ElementAt(0).Should().NotBeNull();
        firstEvent.Odds.ElementAt(0).Bookmaker.Should().NotBeNull().And.Be("betclic");
        firstEvent.Odds.ElementAt(0).Away.Should().Be(4.08);
        firstEvent.Odds.ElementAt(0).Draw.Should().Be(3.82);
        firstEvent.Odds.ElementAt(0).Home.Should().Be(1.7);
        firstEvent.Odds.ElementAt(1).Should().NotBeNull();
        firstEvent.Odds.ElementAt(1).Bookmaker.Should().NotBeNull().And.Be("sport888");
        firstEvent.Odds.ElementAt(1).Away.Should().Be(4.33);
        firstEvent.Odds.ElementAt(1).Draw.Should().Be(4.33);
        firstEvent.Odds.ElementAt(1).Home.Should().Be(1.7);

        var secondEvent = upcomingEvents.ElementAt(1);

        secondEvent.Should().NotBeNull();
        secondEvent.AwayTeam.Should().NotBeNull().And.Be(TestAnonymous2Builder.DefaultAwayTeam);
        secondEvent.CommenceTime.Should().Be(TestAnonymous2Builder.DefaultCommenceTime);
        secondEvent.HomeTeam.Should().NotBeNull().And.Be(TestAnonymous2Builder.DefaultHomeTeam);
        secondEvent.Id.Should().NotBeNull().And.Be("1766194919f1cbfbd846576434f0499b");
        secondEvent.Timestamp.Should().Be(timestamp);
        secondEvent.TraceId.Should().Be(traceId);
        secondEvent.Odds.Should().NotBeNull().And.HaveCount(2);
        secondEvent.Odds.ElementAt(0).Should().NotBeNull();
        secondEvent.Odds.ElementAt(0).Bookmaker.Should().NotBeNull().And.Be("betclic");
        secondEvent.Odds.ElementAt(0).Away.Should().Be(4.08);
        secondEvent.Odds.ElementAt(0).Draw.Should().Be(3.82);
        secondEvent.Odds.ElementAt(0).Home.Should().Be(1.7);
        secondEvent.Odds.ElementAt(1).Bookmaker.Should().NotBeNull().And.Be("sport888");
        secondEvent.Odds.ElementAt(1).Away.Should().Be(4.33);
        secondEvent.Odds.ElementAt(1).Draw.Should().Be(4.33);
        secondEvent.Odds.ElementAt(1).Home.Should().Be(1.7);
    }

    [Test]
    public void ToUpcomingEvents_WithEmptyOddList_ReturnsEmptyConvertedEventsList()
    {
        var converter = new OddsApiObjectConverter();

        var timestamp = DateTime.UtcNow;
        var traceId = Guid.NewGuid();

        List<Anonymous2> rawUpcomingEvents = [];

        var upcomingEvents = converter.ToUpcomingEvents(rawUpcomingEvents, traceId, timestamp);

        upcomingEvents.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToUpcomingEvents_WithNullEvents_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(null, Guid.NewGuid(), DateTime.UtcNow);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("events");
    }

    [Test]
    public void ToUpcomingEvents_WithNullEvent_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents = [null!];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("upcomingEvent");
    }

    [Test]
    public void ToUpcomingEvents_WithNullBookmakers_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents = [
            new TestAnonymous2Builder().SetDefaults().SetBookmakers(null).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("bookmakers");
    }

    [TestCase("")]
    [TestCase(null)]
    public void ToUpcomingEvents_WithNullOrEmptyAwayTeam_ThrowsException(string? awayTeam)
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents = [
            new TestAnonymous2Builder().SetDefaults().SetAwayTeam(awayTeam).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("")]
    [TestCase(null)]
    public void ToUpcomingEvents_WithNullOrEmptyHomeTeam_ThrowsException(string? homeTeam)
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new TestAnonymous2Builder().SetDefaults().SetHomeTeam(homeTeam).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [TestCase("")]
    [TestCase(null)]
    public void ToUpcomingEvents_WithNullOrEmptyBookmakerKey_ThrowsException(string? bookmaker)
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new TestAnonymous2Builder().SetDefaults().SetBookmakers(
                 [
                    new()
                    {
                        Key = bookmaker,
                        Markets =
                        [
                            new()
                            {
                                Key = Markets2Key.H2h,
                                Outcomes =
                                [
                                    new() { Name = "Liverpool", Price = 4.08 },
                                    new() { Name = "Manchester City", Price = 1.7 },
                                    new() { Name = "Draw", Price = 3.82 }
                                ]
                            }
                        ]
                    }
                ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(bookmaker));
    }

    [Test]
    public void ToUpcomingEvents_WithNullMarkets_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new TestAnonymous2Builder().SetDefaults().SetBookmakers(
                [new() { Key = "onexbet", Markets = null }]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("markets");
    }

    [Test]
    public void ToUpcomingEvents_WithNullMarketKey_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new TestAnonymous2Builder().SetDefaults().SetBookmakers(
                [
                    new()
                    {
                        Key = "onexbet",
                        Markets =
                        [
                            new()
                            {
                                Key = null,
                                Outcomes =
                                [
                                    new() { Name = "Liverpool", Price = 4.33 },
                                    new() { Name = "Manchester City", Price = 1.7 },
                                    new() { Name = "Draw", Price = 4.33 }
                                ]
                            }
                        ]
                    }
                ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("markets");
    }

    [Test]
    public void ToUpcomingEvents_WithEmptyMarkets_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new TestAnonymous2Builder().SetDefaults().SetBookmakers(
                [new() { Key = "onexbet", Markets = [] }]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("markets");
    }

    [Test]
    public void ToUpcomingEvents_WithNullOutcomes_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents = [
            new TestAnonymous2Builder().SetDefaults().SetBookmakers(
                 [
                    new()
                    {
                        Key = "onexbet",
                        Markets = [new() { Key = Markets2Key.H2h, Outcomes = null }]
                    }
                 ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("outcomes");
    }

    [Test]
    public void ToUpcomingEvents_WithEmptyOutcomes_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents = [
            new TestAnonymous2Builder().SetDefaults().SetBookmakers(
                 [
                    new()
                    {
                        Key = "onexbet",
                        Markets = [
                            new() { Key = Markets2Key.H2h, Outcomes = [] }
                        ]
                    }
                 ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("outcomes");
    }

    [Test]
    public void ToUpcomingEvents_WithOneOutcome_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents = [
            new TestAnonymous2Builder().SetDefaults().SetBookmakers(
                 [
                    new()
                    {
                        Key = "onexbet",
                        Markets = [
                            new()
                            {
                                Key = Markets2Key.H2h,
                                Outcomes = [
                                    new() { Name = "Manchester City", Price = 1.7 },
                                    new() { Name = "Draw", Price = 4.33 }
                                ]
                            }
                        ]
                    }
                 ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("outcomes");
    }

    [Test]
    public void ToUpcomingEvents_WithNullHomePrice_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new TestAnonymous2Builder().SetDefaults().SetBookmakers(
                [
                    new()
                    {
                        Key = "onexbet",
                        Markets = [
                            new()
                            {
                                Key = Markets2Key.H2h,
                                Outcomes = [
                                    new() { Name = "Liverpool", Price = 4.08 },
                                    new() { Name = "Manchester City", Price = null },
                                    new() { Name = "Draw", Price = 3.82 }
                                ]
                            }
                        ]
                    }
                ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("home");
    }

    [Test]
    public void ToUpcomingEvents_WithNullAwayPrice_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents = [
            new TestAnonymous2Builder().SetDefaults().SetBookmakers(
                 [
                    new()
                    {
                        Key = "onexbet",
                        Markets = [
                            new()
                            {
                                Key = Markets2Key.H2h,
                                Outcomes = [
                                    new() { Name = "Liverpool", Price = null },
                                    new() { Name = "Manchester City", Price = 1.7 },
                                    new() { Name = "Draw", Price = 3.82 }
                                ]
                            }
                        ]
                    }
                 ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("away");
    }

    [Test]
    public void ToUpcomingEvents_WithNullDrawPrice_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents = [
            new TestAnonymous2Builder().SetDefaults().SetBookmakers(
                 [
                    new()
                    {
                        Key = "onexbet",
                        Markets = [
                            new()
                            {
                                Key = Markets2Key.H2h,
                                Outcomes = [
                                    new() { Name = "Liverpool", Price = 4.08 },
                                    new() { Name = "Manchester City", Price = 1.7 },
                                    new() { Name = "Draw", Price = null }
                                ]
                            }
                        ]
                    }
                 ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("draw");
    }

    [Test]
    public void ToEventResults_WithCompletedEvents_ReturnsConvertedEvents()
    {
        var converter = new OddsApiObjectConverter();

        var timestamp = DateTime.UtcNow;
        var traceId = Guid.NewGuid();

        List<Anonymous3> rawEventResults = [
            new TestAnonymous3Builder().SetDefaults().SetScores([
                new() { Name = "Manchester City", Score = "1" },
                new() { Name = "Liverpool", Score = "0" }
            ]).Instance,
            new TestAnonymous3Builder().SetDefaults().SetScores([
                new() { Name = "Manchester City", Score = "0" },
                new() { Name = "Liverpool", Score = "1" }
            ]).Instance,
            new TestAnonymous3Builder().SetDefaults().SetScores([
                new() { Name = "Manchester City", Score = "1" },
                new() { Name = "Liverpool", Score = "1" }
            ]).Instance
        ];

        var results = converter.ToEventResults(rawEventResults, traceId, timestamp).ToList();

        results.Should().NotBeNull().And.HaveCount(3);

        var firstResult = results.ElementAt(0);

        firstResult.Should().NotBeNull();
        firstResult.CommenceTime.Should().Be(TestAnonymous3Builder.DefaultCommenceTime);
        firstResult.Id.Should().NotBeNull().And.Be(TestAnonymous3Builder.DefaultId);
        firstResult.Timestamp.Should().Be(timestamp);
        firstResult.TraceId.Should().Be(traceId);
        firstResult.Winner.Should().NotBeNull().And.Be("Manchester City");

        var secondResult = results.ElementAt(1);

        secondResult.Should().NotBeNull();
        secondResult.CommenceTime.Should().Be(TestAnonymous3Builder.DefaultCommenceTime);
        secondResult.Id.Should().NotBeNull().And.Be(TestAnonymous3Builder.DefaultId);
        secondResult.Timestamp.Should().Be(timestamp);
        secondResult.TraceId.Should().Be(traceId);
        secondResult.Winner.Should().NotBeNull().And.Be("Liverpool");

        var thirdResult = results.ElementAt(2);

        thirdResult.Should().NotBeNull();
        thirdResult.CommenceTime.Should().Be(TestAnonymous3Builder.DefaultCommenceTime);
        thirdResult.Id.Should().NotBeNull().And.Be(TestAnonymous3Builder.DefaultId);
        thirdResult.Timestamp.Should().Be(timestamp);
        thirdResult.TraceId.Should().Be(traceId);
        thirdResult.Winner.Should().NotBeNull().And.Be(Constants.Draw);
    }

    [Test]
    public void ToEventResults_WithEmptyCompletedEventsList_ReturnsEmptyConvertedEventsList()
    {
        var converter = new OddsApiObjectConverter();

        var timestamp = DateTime.UtcNow;
        var traceId = Guid.NewGuid();

        List<Anonymous3> rawEventResults = [];

        var eventResults = converter.ToEventResults(rawEventResults, traceId, timestamp);

        eventResults.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToEventResults_WithNullEvents_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        Action action = () => converter.ToEventResults(null, Guid.NewGuid(), DateTime.UtcNow);

        action.Should().Throw<ArgumentNullException>().WithParameterName("events");
    }

    [Test]
    public void ToEventResults_WithUncompletedEvents_ReturnsNoEvents()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults = [
            new TestAnonymous3Builder().SetDefaults().SetCompleted(null).Instance,
            new TestAnonymous3Builder().SetDefaults().SetCompleted(false).Instance
        ];

        var results = converter.ToEventResults(rawEventResults, Guid.NewGuid(), DateTime.UtcNow);

        results.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToEventResults_WithEmptyScores_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults = [
            new TestAnonymous3Builder().SetDefaults().SetScores(null).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("scores");
    }

    [TestCase("")]
    [TestCase(null)]
    public void ToEventResults_WithNullOrEmptyHomeTeam_ThrowsException(string? homeTeam)
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults = [
            new TestAnonymous3Builder().SetDefaults().SetHomeTeam(homeTeam).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [TestCase("")]
    [TestCase(null)]
    public void ToEventResults_WithNullOrEmptyAwayTeam_ThrowsException(string? awayTeam)
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults = [
            new TestAnonymous3Builder().SetDefaults().SetAwayTeam(awayTeam).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("")]
    [TestCase(null)]
    public void ToEventResults_WithNullOrEmptyTeamName_ThrowsException(string? name)
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults = [
            new TestAnonymous3Builder().SetDefaults().SetScores([
                new() { Name = name, Score = "1" },
                new() { Name = name, Score = "1" }
            ]).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(name));
    }

    [TestCase("")]
    [TestCase(null)]
    public void ToEventResults_WithNullOrEmptyScoreValue_ThrowsException(string? score)
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults = [
            new TestAnonymous3Builder().SetDefaults().SetScores([
                new() { Name = "Manchester City", Score = score },
                new() { Name = "Liverpool", Score = score }
            ]).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(score));
    }

    [Test]
    public void ToEventResults_WithDuplicatedScore_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults = [
            new TestAnonymous3Builder().SetDefaults().SetScores([
                new() { Name = "Manchester City", Score = "1" },
                new() { Name = "Liverpool", Score = "1" },
                new() { Name = "Liverpool", Score = "1" }
            ]).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ToEventResults_WithExtraScore_ReturnsEventResult()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults = [
            new TestAnonymous3Builder().SetDefaults().SetScores([
                new() { Name = "Manchester City", Score = "1" },
                new() { Name = "Liverpool", Score = "0" },
                new() { Name = "Nottingham Forest", Score = "1" }
            ]).Instance
        ];

        var result = converter.ToEventResults(rawEventResults, Guid.NewGuid(), DateTime.UtcNow).ToList();

        result.Should().NotBeNull().And.HaveCount(1);
        result[0].Should().NotBeNull();
        result[0].Winner.Should().NotBeNull().And.Be("Manchester City");
    }

    [Test]
    public void ToEventResults_WithoutAwayTeamScore_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults = [
            new TestAnonymous3Builder().SetDefaults().SetScores([
                new() { Name = "Manchester City", Score = "1" }
            ]).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("scores");
    }

    [Test]
    public void ToEventResults_WithoutHomeTeamScore_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults = [
            new TestAnonymous3Builder().SetDefaults().SetScores([
                new() { Name = "Liverpool", Score = "1" }
            ]).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, Guid.NewGuid(), DateTime.UtcNow).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("scores");
    }
}
