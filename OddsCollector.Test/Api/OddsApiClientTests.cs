namespace OddsCollector.Test.Api;

using Common;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Mocks;
using Moq;
using NUnit.Framework;
using OddsCollector.Api.OddsApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[TestFixture]
public class OddsApiClientTests
{
    private static Mock<IConfiguration> GetConfigurationMock(string apiKey)
    {
        var result = new Mock<IConfiguration>();

        result.Setup(m => m["OddsApi:ApiKey"]).Returns(apiKey);

        return result;
    }

    private static Mock<IClient> GetOddsApiClientMock(ICollection<Anonymous2> events, ICollection<Anonymous3> scores)
    {
        var result = new Mock<IClient>();

        result.Setup(
            m => m.OddsAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<Regions>(),
                It.IsAny<Markets>(),
                It.IsAny<DateFormat>(),
                It.IsAny<OddsFormat>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(events);

        result.Setup(
                m => m.ScoresAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
            .ReturnsAsync(scores);

        return result;
    }

    private static IOddsApiObjectConverter GetRealOddsApiConverter()
    {
        return new OddsApiObjectConverter(TestMocks.GetLoggerMock<OddsApiObjectConverter>().Object);
    }

    [Test]
    public async Task TestScores()
    {
        var scores = new List<Anonymous3>
        {
            new()
            {
                Id = "b8776637057325ea7faf2ab37b6e0bb0",
                Away_team = "Away_team",
                Completed = true,
                Home_team = "Home_team",
                Sport_key = "soccer_epl",
                Scores = new List<ScoreModel>
                {
                    new()
                    {
                        Name = "Away_team",
                        Score = "1"
                    },
                    new()
                    {
                        Name = "Home_team",
                        Score = "2"
                    }
                }
            },
            new()
            {
                Id = "b8e75b4c650603779de82d48eabb555b",
                Away_team = "Away_team",
                Completed = true,
                Home_team = "Home_team",
                Sport_key = "soccer_epl",
                Scores = new List<ScoreModel>
                {
                    new()
                    {
                        Name = "Away_team",
                        Score = "2"
                    },
                    new()
                    {
                        Name = "Home_team",
                        Score = "1"
                    }
                }
            },
            new()
            {
                Id = "96c5e6da25184b93d0e8b30361a87d53",
                Away_team = "Away_team",
                Completed = true,
                Home_team = "Home_team",
                Sport_key = "soccer_epl",
                Scores = new List<ScoreModel>
                {
                    new()
                    {
                        Name = "Away_team",
                        Score = "2"
                    },
                    new()
                    {
                        Name = "Home_team",
                        Score = "2"
                    }
                }
            }
        };

        var adapter = 
            new OddsApiAdapter(
                GetConfigurationMock("somekey").Object,
                GetOddsApiClientMock(null!, scores).Object,
                GetRealOddsApiConverter());

        var result = await
            adapter.GetCompletedEventsAsync(new List<string> { "soccer_epl" });

        result.Should().Contain(
            new KeyValuePair<string, string?>("b8776637057325ea7faf2ab37b6e0bb0", "Home_team"));
        result.Should().Contain(
            new KeyValuePair<string, string?>("b8e75b4c650603779de82d48eabb555b", "Away_team"));
        result.Should().Contain(
            new KeyValuePair<string, string?>("96c5e6da25184b93d0e8b30361a87d53", "Draw"));
    }

    [Test]
    public async Task TestNullScores()
    {
        var scores = new List<Anonymous3>
        {
            new()
            {
                Id = "b8776637057325ea7faf2ab37b6e0bb0",
                Away_team = null,
                Completed = true,
                Home_team = "Home_team",
                Sport_key = "soccer_epl",
                Scores = new List<ScoreModel>
                {
                    new()
                    {
                        Name = "Away_team",
                        Score = "1"
                    },
                    new()
                    {
                        Name = "Home_team",
                        Score = "2"
                    }
                }
            },
            new()
            {
                Id = "b8e75b4c650603779de82d48eabb555b",
                Away_team = "Away_team",
                Completed = true,
                Home_team = null,
                Sport_key = "soccer_epl",
                Scores = new List<ScoreModel>
                {
                    new()
                    {
                        Name = "Away_team",
                        Score = "2"
                    },
                    new()
                    {
                        Name = "Home_team",
                        Score = "1"
                    }
                }
            },
            new()
            {
                Id = "96c5e6da25184b93d0e8b30361a87d53",
                Away_team = "Away_team",
                Completed = true,
                Home_team = "Home_team",
                Sport_key = "soccer_epl",
                Scores = new List<ScoreModel>
                {
                    null!,
                    new()
                    {
                        Name = "Home_team",
                        Score = "2"
                    }
                }
            },
            new()
            {
                Id = "a9af080b4e85ccb2c39517e0902f282b",
                Away_team = "Away_team",
                Completed = false,
                Home_team = "Home_team",
                Sport_key = "soccer_epl",
                Scores = null
            },
            new()
            {
                Id = "d59ba83d9632e474585f115db3a27bce",
                Away_team = "Away_team",
                Completed = true,
                Home_team = "Home_team",
                Sport_key = "soccer_germany_bundesliga",
                Scores = new List<ScoreModel>
                {
                    new()
                    {
                        Name = null,
                        Score = "2"
                    },
                    new()
                    {
                        Name = "Home_team",
                        Score = null
                    }
                }
            }
        };

        var adapter =
            new OddsApiAdapter(
                GetConfigurationMock("somekey").Object,
                GetOddsApiClientMock(null!, scores).Object,
                GetRealOddsApiConverter());

        await adapter.GetCompletedEventsAsync(new List<string> { "soccer_epl" });
    }

    [Test]
    public async Task TestEvents()
    {
        var dateTime = DateTime.Today;

        var events = new List<Anonymous2>
        {
            new()
            {
                Id = "b916757066ac373bc2ee361acbeb4368",
                Away_team = "West Ham United",
                Home_team = "Manchester City",
                Sport_key = "soccer_epl",
                Commence_time = dateTime,
                Bookmakers = new List<Bookmakers>
                {
                    new()
                    {
                        Markets = new List<Markets2>
                        {
                            new()
                            {
                                Key = Markets2Key.H2h,
                                Outcomes = new List<Outcome>
                                {
                                    new()
                                    {
                                        Name = "West Ham United",
                                        Price = 1
                                    },
                                    new()
                                    {
                                        Name = "Manchester City",
                                        Price = 2
                                    },
                                    new()
                                    {
                                        Name = Constants.Draw,
                                        Price = 3
                                    }
                                }
                            }
                        },
                        Key = "unibet",
                        Last_update = dateTime
                    }
                }
            }
        };

        var adapter =
            new OddsApiAdapter(
                GetConfigurationMock("somekey").Object,
                GetOddsApiClientMock(events, null!).Object,
                GetRealOddsApiConverter());

        var result = await
            adapter.GetUpcomingEventsAsync(new List<string> { "soccer_epl" });

        var sportEvents = result.ToList();

        sportEvents
            .Should().ContainSingle(
                x => x.SportEventId == "b916757066ac373bc2ee361acbeb4368");

        var singleEvent = 
            sportEvents.First(
                x => x.SportEventId == "b916757066ac373bc2ee361acbeb4368");

        singleEvent.AwayTeam.Should().Be("West Ham United");
        singleEvent.HomeTeam.Should().Be("Manchester City");
        singleEvent.LeagueId.Should().Be("soccer_epl");
        singleEvent.CommenceTime.Should().Be(dateTime);
        singleEvent.Outcome.Should().BeNull();

        singleEvent.Odds.Should().ContainSingle(x =>
            x.Bookmaker == "unibet" && x.LastUpdate == dateTime &&
            x.SportEventId == "b916757066ac373bc2ee361acbeb4368");

        var odd = 
            singleEvent.Odds!.First(x => 
                x.Bookmaker == "unibet" && 
                x.LastUpdate == dateTime && 
                x.SportEventId == "b916757066ac373bc2ee361acbeb4368");

        odd.DrawOdd.Should().Be(3);
        odd.HomeOdd.Should().Be(2);
        odd.AwayOdd.Should().Be(1);
    }

    [Test]
    public async Task TestNoEvents()
    {
        var adapter =
            new OddsApiAdapter(
                GetConfigurationMock("somekey").Object,
                GetOddsApiClientMock(new List<Anonymous2>(), new List<Anonymous3>()).Object,
                GetRealOddsApiConverter());

        await adapter.GetUpcomingEventsAsync(new List<string> { "soccer_epl" });
    }

    [Test]
    public async Task TestScoresNullEvents()
    {
        var adapter =
            new OddsApiAdapter(
                GetConfigurationMock("somekey").Object,
                GetOddsApiClientMock(new List<Anonymous2>(), new List<Anonymous3>()).Object,
                GetRealOddsApiConverter());

        Func<Task> func = () => adapter.GetUpcomingEventsAsync(null!);
        await func.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task TestNoScores()
    {
        var adapter =
            new OddsApiAdapter(
                GetConfigurationMock("somekey").Object,
                GetOddsApiClientMock(new List<Anonymous2>(), new List<Anonymous3>()).Object,
                GetRealOddsApiConverter());

        await adapter.GetCompletedEventsAsync(new List<string> { "soccer_epl" });
    }

    [Test]
    public async Task TestScoresNullLeagues()
    {
        var adapter =
            new OddsApiAdapter(
                GetConfigurationMock("somekey").Object,
                GetOddsApiClientMock(new List<Anonymous2>(), new List<Anonymous3>()).Object,
                GetRealOddsApiConverter());

        Func<Task> func = () => adapter.GetCompletedEventsAsync(null!);
        await func.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public void TestEmptyApiKey()
    {
        var action = () =>
        {
            _ = new OddsApiAdapter(
                GetConfigurationMock("").Object,
                GetOddsApiClientMock(new List<Anonymous2>(), new List<Anonymous3>()).Object,
                GetRealOddsApiConverter());
        };

        action.Should().Throw<Exception>();
    }

    [Test]
    public void TestNullApiKey()
    {
        var action = () =>
        {
            _ = new OddsApiAdapter(
                GetConfigurationMock(null!).Object,
                GetOddsApiClientMock(new List<Anonymous2>(), new List<Anonymous3>()).Object,
                GetRealOddsApiConverter());
        };

        action.Should().Throw<Exception>();
    }
}