using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.Converter;
using OddsCollector.Functions.OddsApi.WebApi;
using OddsCollector.Functions.Tests.Data;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converter;

[Parallelizable(ParallelScope.All)]
internal class OddsApiObjectConverterTests
{
    [Test]
    public void ToUpcomingEvents_WithOddList_ReturnsConvertedEvents()
    {
        var converter = new OddsApiObjectConverter();

        const string secondId = "1766194919f1cbfbd846576434f0499b";

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().Instance,
            new Anonymous2Builder().SetSampleData().SetId(secondId).Instance
        ];

        var upcomingEvents = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp)
            .ToList();

        upcomingEvents.Should().NotBeNull().And.HaveCount(2);

        var firstEvent = upcomingEvents.ElementAt(0);

        firstEvent.Should().NotBeNull();
        firstEvent.AwayTeam.Should().NotBeNull().And.Be(SampleEvent.AwayTeam);
        firstEvent.CommenceTime.Should().Be(SampleEvent.CommenceTime);
        firstEvent.HomeTeam.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
        firstEvent.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
        firstEvent.Timestamp.Should().Be(SampleEvent.Timestamp);
        firstEvent.TraceId.Should().Be(SampleEvent.TraceId);
        firstEvent.Odds.Should().NotBeNull().And.HaveCount(SampleEvent.Bookmakers.Count);
        firstEvent.Odds.ElementAt(0).Should().NotBeNull();
        firstEvent.Odds.ElementAt(0).Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker1);
        firstEvent.Odds.ElementAt(0).Away.Should().Be(SampleEvent.AwayOdd1);
        firstEvent.Odds.ElementAt(0).Draw.Should().Be(SampleEvent.DrawOdd1);
        firstEvent.Odds.ElementAt(0).Home.Should().Be(SampleEvent.HomeOdd1);
        firstEvent.Odds.ElementAt(1).Should().NotBeNull();
        firstEvent.Odds.ElementAt(1).Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker2);
        firstEvent.Odds.ElementAt(1).Away.Should().Be(SampleEvent.AwayOdd2);
        firstEvent.Odds.ElementAt(1).Draw.Should().Be(SampleEvent.DrawOdd2);
        firstEvent.Odds.ElementAt(1).Home.Should().Be(SampleEvent.HomeOdd2);
        firstEvent.Odds.ElementAt(2).Should().NotBeNull();
        firstEvent.Odds.ElementAt(2).Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker3);
        firstEvent.Odds.ElementAt(2).Away.Should().Be(SampleEvent.AwayOdd3);
        firstEvent.Odds.ElementAt(2).Draw.Should().Be(SampleEvent.DrawOdd3);
        firstEvent.Odds.ElementAt(2).Home.Should().Be(SampleEvent.HomeOdd3);

        var secondEvent = upcomingEvents.ElementAt(1);

        secondEvent.Should().NotBeNull();
        secondEvent.AwayTeam.Should().NotBeNull().And.Be(SampleEvent.AwayTeam);
        secondEvent.CommenceTime.Should().Be(SampleEvent.CommenceTime);
        secondEvent.HomeTeam.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
        secondEvent.Id.Should().NotBeNull().And.Be(secondId);
        secondEvent.Timestamp.Should().Be(SampleEvent.Timestamp);
        secondEvent.TraceId.Should().Be(SampleEvent.TraceId);
        secondEvent.Odds.Should().NotBeNull().And.HaveCount(3);
        secondEvent.Odds.ElementAt(0).Should().NotBeNull();
        secondEvent.Odds.ElementAt(0).Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker1);
        secondEvent.Odds.ElementAt(0).Away.Should().Be(SampleEvent.AwayOdd1);
        secondEvent.Odds.ElementAt(0).Draw.Should().Be(SampleEvent.DrawOdd1);
        secondEvent.Odds.ElementAt(0).Home.Should().Be(SampleEvent.HomeOdd1);
        secondEvent.Odds.ElementAt(1).Should().NotBeNull();
        secondEvent.Odds.ElementAt(1).Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker2);
        secondEvent.Odds.ElementAt(1).Away.Should().Be(SampleEvent.AwayOdd2);
        secondEvent.Odds.ElementAt(1).Draw.Should().Be(SampleEvent.DrawOdd2);
        secondEvent.Odds.ElementAt(1).Home.Should().Be(SampleEvent.HomeOdd2);
        secondEvent.Odds.ElementAt(2).Should().NotBeNull();
        secondEvent.Odds.ElementAt(2).Bookmaker.Should().NotBeNull().And.Be(SampleEvent.Bookmaker3);
        secondEvent.Odds.ElementAt(2).Away.Should().Be(SampleEvent.AwayOdd3);
        secondEvent.Odds.ElementAt(2).Draw.Should().Be(SampleEvent.DrawOdd3);
        secondEvent.Odds.ElementAt(2).Home.Should().Be(SampleEvent.HomeOdd3);
    }

    [Test]
    public void ToUpcomingEvents_WithEmptyOddList_ReturnsEmptyConvertedEventsList()
    {
        var converter = new OddsApiObjectConverter();

        var upcomingEvents = converter.ToUpcomingEvents([], SampleEvent.TraceId, SampleEvent.Timestamp);

        upcomingEvents.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToUpcomingEvents_WithNullEvents_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(null, SampleEvent.TraceId, SampleEvent.Timestamp);
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("events");
    }

    [Test]
    public void ToUpcomingEvents_WithNullEvent_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        var action = () =>
        {
            _ = converter.ToUpcomingEvents([null!], SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("upcomingEvent");
    }

    [Test]
    public void ToUpcomingEvents_WithNullBookmakers_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().SetBookmakers(null).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("bookmakers");
    }

    [TestCase("")]
    [TestCase(null)]
    public void ToUpcomingEvents_WithNullOrEmptyAwayTeam_ThrowsException(string? awayTeam)
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().SetAwayTeam(awayTeam).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
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
            new Anonymous2Builder().SetSampleData().SetHomeTeam(homeTeam).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
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
            new Anonymous2Builder().SetSampleData().SetBookmakers(
                [
                    new Bookmakers
                    {
                        Key = bookmaker,
                        Markets =
                        [
                            new Markets2 { Key = Markets2Key.H2h, Outcomes = SampleEvent.Outcomes1 }
                        ]
                    }
                ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(bookmaker));
    }

    [Test]
    public void ToUpcomingEvents_WithNullMarkets_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().SetBookmakers(
                [new Bookmakers { Key = SampleEvent.Bookmaker1, Markets = null }]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("markets");
    }

    [Test]
    public void ToUpcomingEvents_WithNullMarketKey_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().SetBookmakers(
                [
                    new Bookmakers
                    {
                        Key = SampleEvent.Bookmaker1,
                        Markets =
                        [
                            new Markets2 { Key = null, Outcomes = SampleEvent.Outcomes1 }
                        ]
                    }
                ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("markets");
    }

    [Test]
    public void ToUpcomingEvents_WithEmptyMarkets_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().SetBookmakers(
                [new Bookmakers { Key = SampleEvent.Bookmaker1, Markets = [] }]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("markets");
    }

    [Test]
    public void ToUpcomingEvents_WithNullOutcomes_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().SetBookmakers(
                [
                    new Bookmakers
                    {
                        Key = SampleEvent.Bookmaker1,
                        Markets = [new Markets2 { Key = Markets2Key.H2h, Outcomes = null }]
                    }
                ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("outcomes");
    }

    [Test]
    public void ToUpcomingEvents_WithEmptyOutcomes_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().SetBookmakers(
                [
                    new Bookmakers
                    {
                        Key = SampleEvent.Bookmaker1,
                        Markets =
                        [
                            new Markets2 { Key = Markets2Key.H2h, Outcomes = [] }
                        ]
                    }
                ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("outcomes");
    }

    [Test]
    public void ToUpcomingEvents_WithoutOneOutcome_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().SetBookmakers(
                [
                    new Bookmakers
                    {
                        Key = SampleEvent.Bookmaker1,
                        Markets =
                        [
                            new Markets2
                            {
                                Key = Markets2Key.H2h,
                                Outcomes =
                                [
                                    SampleEvent.HomeOutcome1,
                                    SampleEvent.DrawOutcome1
                                ]
                            }
                        ]
                    }
                ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("outcomes");
    }

    [Test]
    public void ToUpcomingEvents_WithNullHomePrice_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().SetBookmakers(
                [
                    new Bookmakers
                    {
                        Key = SampleEvent.Bookmaker1,
                        Markets =
                        [
                            new Markets2
                            {
                                Key = Markets2Key.H2h,
                                Outcomes =
                                [
                                    SampleEvent.AwayOutcome1,
                                    new Outcome { Name = SampleEvent.HomeTeam, Price = null },
                                    SampleEvent.DrawOutcome1
                                ]
                            }
                        ]
                    }
                ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("home");
    }

    [Test]
    public void ToUpcomingEvents_WithNullAwayPrice_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().SetBookmakers(
                [
                    new Bookmakers
                    {
                        Key = SampleEvent.Bookmaker1,
                        Markets =
                        [
                            new Markets2
                            {
                                Key = Markets2Key.H2h,
                                Outcomes =
                                [
                                    new Outcome { Name = SampleEvent.AwayTeam, Price = null },
                                    SampleEvent.HomeOutcome1,
                                    SampleEvent.DrawOutcome1
                                ]
                            }
                        ]
                    }
                ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("away");
    }

    [Test]
    public void ToUpcomingEvents_WithNullDrawPrice_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().SetBookmakers(
                [
                    new Bookmakers
                    {
                        Key = SampleEvent.Bookmaker1,
                        Markets =
                        [
                            new Markets2
                            {
                                Key = Markets2Key.H2h,
                                Outcomes =
                                [
                                    SampleEvent.AwayOutcome1,
                                    SampleEvent.HomeOutcome1,
                                    new Outcome { Name = Constants.Draw, Price = null }
                                ]
                            }
                        ]
                    }
                ]
            ).Instance
        ];

        var action = () =>
        {
            _ = converter.ToUpcomingEvents(rawUpcomingEvents, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("draw");
    }

    [Test]
    public void ToEventResults_WithCompletedEvents_ReturnsConvertedEvents()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetScores([
                new ScoreModel { Name = SampleEvent.HomeTeam, Score = "1" },
                new ScoreModel { Name = SampleEvent.AwayTeam, Score = "0" }
            ]).Instance,
            new Anonymous3Builder().SetSampleData().SetScores([
                new ScoreModel { Name = SampleEvent.HomeTeam, Score = "0" },
                new ScoreModel { Name = SampleEvent.AwayTeam, Score = "1" }
            ]).Instance,
            new Anonymous3Builder().SetSampleData().SetScores([
                new ScoreModel { Name = SampleEvent.HomeTeam, Score = "1" },
                new ScoreModel { Name = SampleEvent.AwayTeam, Score = "1" }
            ]).Instance
        ];

        var results = converter.ToEventResults(rawEventResults, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();

        results.Should().NotBeNull().And.HaveCount(3);

        var firstResult = results.ElementAt(0);

        firstResult.Should().NotBeNull();
        firstResult.CommenceTime.Should().Be(SampleEvent.CommenceTime);
        firstResult.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
        firstResult.Timestamp.Should().Be(SampleEvent.Timestamp);
        firstResult.TraceId.Should().Be(SampleEvent.TraceId);
        firstResult.Winner.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);

        var secondResult = results.ElementAt(1);

        secondResult.Should().NotBeNull();
        secondResult.CommenceTime.Should().Be(SampleEvent.CommenceTime);
        secondResult.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
        secondResult.Timestamp.Should().Be(SampleEvent.Timestamp);
        secondResult.TraceId.Should().Be(SampleEvent.TraceId);
        secondResult.Winner.Should().NotBeNull().And.Be(SampleEvent.AwayTeam);

        var thirdResult = results.ElementAt(2);

        thirdResult.Should().NotBeNull();
        thirdResult.CommenceTime.Should().Be(SampleEvent.CommenceTime);
        thirdResult.Id.Should().NotBeNull().And.Be(SampleEvent.Id);
        thirdResult.Timestamp.Should().Be(SampleEvent.Timestamp);
        thirdResult.TraceId.Should().Be(SampleEvent.TraceId);
        thirdResult.Winner.Should().NotBeNull().And.Be(Constants.Draw);
    }

    [Test]
    public void ToEventResults_WithEmptyCompletedEventsList_ReturnsEmptyConvertedEventsList()
    {
        var converter = new OddsApiObjectConverter();

        var eventResults = converter.ToEventResults([], SampleEvent.TraceId, SampleEvent.Timestamp);

        eventResults.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToEventResults_WithNullEvents_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        Action action = () => converter.ToEventResults(null, SampleEvent.TraceId, SampleEvent.Timestamp);

        action.Should().Throw<ArgumentNullException>().WithParameterName("events");
    }

    [Test]
    public void ToEventResults_WithUncompletedEvents_ReturnsNoEvents()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetCompleted(null).Instance,
            new Anonymous3Builder().SetSampleData().SetCompleted(false).Instance
        ];

        var results = converter.ToEventResults(rawEventResults, SampleEvent.TraceId, SampleEvent.Timestamp);

        results.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToEventResults_WithNullScores_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetScores(null).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentNullException>().WithParameterName("scores");
    }

    [Test]
    public void ToEventResults_WithEmptyScores_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetScores([]).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("scores");
    }

    [TestCase("")]
    [TestCase(null)]
    public void ToEventResults_WithNullOrEmptyHomeTeam_ThrowsException(string? homeTeam)
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetHomeTeam(homeTeam).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(homeTeam));
    }

    [TestCase("")]
    [TestCase(null)]
    public void ToEventResults_WithNullOrEmptyAwayTeam_ThrowsException(string? awayTeam)
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetAwayTeam(awayTeam).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(awayTeam));
    }

    [TestCase("")]
    [TestCase(null)]
    public void ToEventResults_WithNullOrEmptyTeamName_ThrowsException(string? name)
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetScores([
                new ScoreModel { Name = name, Score = SampleEvent.HomeScore },
                new ScoreModel { Name = name, Score = SampleEvent.AwayScore }
            ]).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(name));
    }

    [TestCase("")]
    [TestCase(null)]
    public void ToEventResults_WithNullOrEmptyScoreValue_ThrowsException(string? score)
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetScores([
                new ScoreModel { Name = SampleEvent.HomeTeam, Score = score },
                new ScoreModel { Name = SampleEvent.AwayTeam, Score = score }
            ]).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(score));
    }

    [Test]
    public void ToEventResults_WithDuplicatedScore_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetScores([
                SampleEvent.HomeScoreModel,
                SampleEvent.AwayScoreModel,
                SampleEvent.AwayScoreModel
            ]).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ToEventResults_WithExtraScore_ReturnsEventResult()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetScores([
                SampleEvent.HomeScoreModel,
                SampleEvent.AwayScoreModel,
                new ScoreModel { Name = "Nottingham Forest", Score = "1" }
            ]).Instance
        ];

        var result = converter.ToEventResults(rawEventResults, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();

        result.Should().NotBeNull().And.HaveCount(1);
        result[0].Should().NotBeNull();
        result[0].Winner.Should().NotBeNull().And.Be(SampleEvent.HomeTeam);
    }

    [Test]
    public void ToEventResults_WithoutAwayTeamScore_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetScores([SampleEvent.HomeScoreModel]).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("scores");
    }

    [Test]
    public void ToEventResults_WithoutHomeTeamScore_ThrowsException()
    {
        var converter = new OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetScores([SampleEvent.AwayScoreModel]).Instance
        ];

        var action = () =>
        {
            _ = converter.ToEventResults(rawEventResults, SampleEvent.TraceId, SampleEvent.Timestamp).ToList();
        };

        action.Should().Throw<ArgumentException>().WithParameterName("scores");
    }
}
