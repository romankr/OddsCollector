using OddsCollector.Functions.Models;
using OddsCollector.Functions.OddsApi.WebApi;
using OddsCollector.Functions.Tests.Infrastructure.Data;
using FunctionsApp = OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converter;

internal sealed class OddsApiObjectConverter
{
    private static readonly
        IEnumerable<(ICollection<Anonymous2>? OriginalEvents, Type ExceptionType, string ParameterName)>
        UpcomingEventTestCases =
            new List<(ICollection<Anonymous2>? OriginalEvents, Type ExceptionType, string ParameterName)>
            {
                (
                    null,
                    typeof(ArgumentNullException),
                    "events"
                ),
                (
                    [null!],
                    typeof(ArgumentNullException),
                    "upcomingEvent"
                ),
                (
                    [new Anonymous2Builder().SetSampleData().SetBookmakers(null).Instance],
                    typeof(ArgumentNullException),
                    "bookmakers"
                ),
                (
                    [new Anonymous2Builder().SetSampleData().SetAwayTeam("").Instance],
                    typeof(ArgumentException),
                    "awayTeam"
                ),
                (
                    [new Anonymous2Builder().SetSampleData().SetAwayTeam(null).Instance],
                    typeof(ArgumentNullException),
                    "awayTeam"
                ),
                (
                    [new Anonymous2Builder().SetSampleData().SetHomeTeam("").Instance],
                    typeof(ArgumentException),
                    "homeTeam"
                ),
                (
                    [new Anonymous2Builder().SetSampleData().SetHomeTeam(null).Instance],
                    typeof(ArgumentNullException),
                    "homeTeam"
                ),
                (
                    [
                        new Anonymous2Builder().SetSampleData().SetBookmakers(
                            [
                                new Bookmakers
                                {
                                    Key = "",
                                    Markets =
                                    [
                                        new Markets2 { Key = Markets2Key.H2h, Outcomes = SampleEvent.Outcomes1 }
                                    ]
                                }
                            ]
                        ).Instance
                    ],
                    typeof(ArgumentException),
                    "bookmaker"
                ),
                (
                    [
                        new Anonymous2Builder().SetSampleData().SetBookmakers(
                            [
                                new Bookmakers
                                {
                                    Key = null,
                                    Markets =
                                    [
                                        new Markets2 { Key = Markets2Key.H2h, Outcomes = SampleEvent.Outcomes1 }
                                    ]
                                }
                            ]
                        ).Instance
                    ],
                    typeof(ArgumentNullException),
                    "bookmaker"
                ),
                (
                    [
                        new Anonymous2Builder().SetSampleData().SetBookmakers(
                            [new Bookmakers { Key = SampleEvent.Bookmaker1, Markets = null }]
                        ).Instance
                    ],
                    typeof(ArgumentNullException),
                    "markets"
                ),
                (
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
                    ],
                    typeof(ArgumentNullException),
                    "markets"
                ),
                (
                    [
                        new Anonymous2Builder().SetSampleData().SetBookmakers(
                            [new Bookmakers { Key = SampleEvent.Bookmaker1, Markets = [] }]
                        ).Instance
                    ],
                    typeof(ArgumentException),
                    "markets"
                ),
                (
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
                    ],
                    typeof(ArgumentNullException),
                    "outcomes"
                ),
                (
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
                    ],
                    typeof(ArgumentException),
                    "outcomes"
                ),
                (
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
                    ],
                    typeof(ArgumentException),
                    "outcomes"
                ),
                (
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
                    ],
                    typeof(ArgumentNullException),
                    "home"
                ),
                (
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
                    ],
                    typeof(ArgumentNullException),
                    "away"
                ),
                (
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
                                                new Outcome { Name = OutcomeTypes.Draw, Price = null }
                                            ]
                                        }
                                    ]
                                }
                            ]
                        ).Instance
                    ],
                    typeof(ArgumentNullException),
                    "draw"
                )
            };

    private static readonly
        IEnumerable<(ICollection<Anonymous3>? OriginalEvents, Type ExceptionType, string ParameterName)>
        EventResultTestCases =
            new List<(ICollection<Anonymous3>? OriginalEvents, Type ExceptionType, string ParameterName)>
            {
                (
                    null,
                    typeof(ArgumentNullException),
                    "events"
                ),
                (
                    [new Anonymous3Builder().SetSampleData().SetScores(null).Instance],
                    typeof(ArgumentNullException),
                    "scores"
                ),
                (
                    [new Anonymous3Builder().SetSampleData().SetScores([]).Instance],
                    typeof(ArgumentException),
                    "scores"
                ),
                (
                    [new Anonymous3Builder().SetSampleData().SetHomeTeam(string.Empty).Instance],
                    typeof(ArgumentException),
                    "homeTeam"
                ),
                (
                    [new Anonymous3Builder().SetSampleData().SetHomeTeam(null).Instance],
                    typeof(ArgumentNullException),
                    "homeTeam"
                ),
                (
                    [new Anonymous3Builder().SetSampleData().SetAwayTeam(string.Empty).Instance],
                    typeof(ArgumentException),
                    "awayTeam"
                ),
                (
                    [new Anonymous3Builder().SetSampleData().SetAwayTeam(null).Instance],
                    typeof(ArgumentNullException),
                    "awayTeam"
                ),
                (
                    [
                        new Anonymous3Builder().SetSampleData().SetScores([
                            new ScoreModel { Name = string.Empty, Score = SampleEvent.HomeScore },
                            new ScoreModel { Name = string.Empty, Score = SampleEvent.AwayScore }
                        ]).Instance
                    ],
                    typeof(ArgumentException),
                    "name"
                ),
                (
                    [
                        new Anonymous3Builder().SetSampleData().SetScores([
                            new ScoreModel { Name = null, Score = SampleEvent.HomeScore },
                            new ScoreModel { Name = null, Score = SampleEvent.AwayScore }
                        ]).Instance
                    ],
                    typeof(ArgumentNullException),
                    "name"
                ),
                (
                    [
                        new Anonymous3Builder().SetSampleData().SetScores([
                            new ScoreModel { Name = SampleEvent.HomeTeam, Score = string.Empty },
                            new ScoreModel { Name = SampleEvent.AwayTeam, Score = string.Empty }
                        ]).Instance
                    ],
                    typeof(ArgumentException),
                    "score"
                ),
                (
                    [
                        new Anonymous3Builder().SetSampleData().SetScores([
                            new ScoreModel { Name = SampleEvent.HomeTeam, Score = null },
                            new ScoreModel { Name = SampleEvent.AwayTeam, Score = null }
                        ]).Instance
                    ],
                    typeof(ArgumentNullException),
                    "score"
                ),
                (
                    [
                        new Anonymous3Builder().SetSampleData().SetScores([
                            SampleEvent.HomeScoreModel,
                            SampleEvent.AwayScoreModel,
                            SampleEvent.AwayScoreModel
                        ]).Instance
                    ],
                    typeof(ArgumentException),
                    "scores"
                ),
                (
                    [new Anonymous3Builder().SetSampleData().SetScores([SampleEvent.HomeScoreModel]).Instance],
                    typeof(ArgumentException),
                    "scores"
                ),
                (
                    [new Anonymous3Builder().SetSampleData().SetScores([SampleEvent.AwayScoreModel]).Instance],
                    typeof(ArgumentException),
                    "scores"
                )
            };

    [Test]
    public void ToUpcomingEvents_WithOddList_ReturnsConvertedEvents()
    {
        // Arrange
        var converter = new FunctionsApp.Converter.OddsApiObjectConverter();

        const string secondId = "1766194919f1cbfbd846576434f0499b";

        List<Anonymous2> rawUpcomingEvents =
        [
            new Anonymous2Builder().SetSampleData().Instance,
            new Anonymous2Builder().SetSampleData().SetId(secondId).Instance
        ];

        // Act
        var upcomingEvents =
            converter.ToUpcomingEvents(rawUpcomingEvents).ToList();

        // Assert
        upcomingEvents.Should().NotBeNull().And.HaveCount(2);

        upcomingEvents.ElementAt(0).Should().NotBeNull()
            .And.BeEquivalentTo(new UpcomingEventBuilder().SetSampleData().Instance);

        upcomingEvents.ElementAt(1).Should().NotBeNull()
            .And.BeEquivalentTo(new UpcomingEventBuilder().SetSampleData().SetId(secondId).Instance);
    }

    [Test]
    public void ToUpcomingEvents_WithEmptyOddList_ReturnsEmptyConvertedEventsList()
    {
        // Arrange
        var converter = new FunctionsApp.Converter.OddsApiObjectConverter();

        // Act
        var upcomingEvents = converter.ToUpcomingEvents([]).ToList();

        // Assert
        upcomingEvents.Should().NotBeNull().And.BeEmpty();
    }

    [TestCaseSource(nameof(UpcomingEventTestCases))]
    public void ToUpcomingEvents_WithInvalidEvents_ThrowsException(
        (ICollection<Anonymous2>? OriginalEvents, Type ExceptionType, string ParameterName) testCase)
    {
        // Arrange
        var converter = new FunctionsApp.Converter.OddsApiObjectConverter();

        // Act & Assert
        Assert.Throws(
            testCase.ExceptionType,
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            () => converter.ToUpcomingEvents(testCase.OriginalEvents).ToList());
    }

    [Test]
    public void ToEventResults_WithCompletedEvents_ReturnsConvertedEvents()
    {
        // Arrange
        var converter = new FunctionsApp.Converter.OddsApiObjectConverter();

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

        // Act
        var eventResults = converter.ToEventResults(rawEventResults).ToList();

        // Assert
        eventResults.Should().NotBeNull().And.HaveCount(3);

        eventResults.ElementAt(0).Should().NotBeNull().And
            .BeEquivalentTo(new EventResultBuilder().SetSampleData().SetWinner(SampleEvent.HomeTeam).Instance);

        eventResults.ElementAt(1).Should().NotBeNull().And
            .BeEquivalentTo(new EventResultBuilder().SetSampleData().SetWinner(SampleEvent.AwayTeam).Instance);

        eventResults.ElementAt(2).Should().NotBeNull().And
            .BeEquivalentTo(new EventResultBuilder().SetSampleData().SetWinner(OutcomeTypes.Draw).Instance);
    }

    [Test]
    public void ToEventResults_WithEmptyCompletedEventsList_ReturnsEmptyConvertedEventsList()
    {
        // Arrange
        var converter = new FunctionsApp.Converter.OddsApiObjectConverter();

        // Act
        var eventResults = converter.ToEventResults([]);

        // Assert
        eventResults.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToEventResults_WithUncompletedEvents_ReturnsNoEvents()
    {
        // Arrange
        var converter = new FunctionsApp.Converter.OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetCompleted(null).Instance,
            new Anonymous3Builder().SetSampleData().SetCompleted(false).Instance
        ];

        // Act
        var eventResults = converter.ToEventResults(rawEventResults);

        // Assert
        eventResults.Should().NotBeNull().And.BeEmpty();
    }

    [Test]
    public void ToEventResults_WithExtraScore_ReturnsEventResult()
    {
        // Arrange
        var converter = new FunctionsApp.Converter.OddsApiObjectConverter();

        List<Anonymous3> rawEventResults =
        [
            new Anonymous3Builder().SetSampleData().SetScores([
                SampleEvent.HomeScoreModel,
                SampleEvent.AwayScoreModel,
                new ScoreModel { Name = "Nottingham Forest", Score = "1" }
            ]).Instance
        ];

        // Act
        var eventResults = converter.ToEventResults(rawEventResults).ToList();

        // Assert
        eventResults.Should().NotBeNull().And.HaveCount(1);

        eventResults.ElementAt(0).Should().NotBeNull().And
            .BeEquivalentTo(new EventResultBuilder().SetSampleData().Instance);
    }

    [TestCaseSource(nameof(EventResultTestCases))]
    public void ToEventResults_WithInvalidEvents_ThrowsException(
        (ICollection<Anonymous3>? OriginalEvents, Type ExceptionType, string ParameterName) testCase)
    {
        // Arrange
        var converter = new FunctionsApp.Converter.OddsApiObjectConverter();

        // Act & Assert
        Assert.Throws(
            testCase.ExceptionType,
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            () => converter.ToEventResults(testCase.OriginalEvents).ToList());
    }
}
