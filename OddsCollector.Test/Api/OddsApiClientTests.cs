namespace OddsCollector.Test.Api;

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OddsCollector.Api.OddsApi;
using System.Collections.Generic;
using System.Threading.Tasks;

[TestFixture]
public class OddsApiClientTests
{
    [Test]
    public async Task TestScores()
    {
        var loggerMock = new Mock<ILogger<OddsApiAdapter>>();
        
        var configurationMock = new Mock<IConfiguration>();
        
        configurationMock
            .Setup(m => m["OddsApi:ApiKey"])
            .Returns("somekey");

        var oddsApiClientMock = new Mock<IClient>();

        var list = new List<Anonymous3>
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
            },
            new()
            {
                Id = "96c5e6da25184b93d0e8b30361a87d53",
                Away_team = "Away_team",
                Completed = false,
                Home_team = "Home_team",
                Sport_key = "soccer_epl",
                Scores = null
            },
            new()
            {
                Id = "b8e75b4c650603779de82d48eabb555b",
                Away_team = "Away_team",
                Completed = true,
                Home_team = "Home_team",
                Sport_key = "soccer_germany_bundesliga",
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
            }
        };

        oddsApiClientMock
            .Setup(
                m => m.ScoresAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<int>()))
            .ReturnsAsync(list);

        var adapter = 
            new OddsApiAdapter(
                configurationMock.Object, 
                oddsApiClientMock.Object, 
                loggerMock.Object);

        var result = await
            adapter.GetCompletedEventsAsync(new List<string> { "soccer_epl" });

        result.Should().Contain(
            new KeyValuePair<string, string?>("b8776637057325ea7faf2ab37b6e0bb0", "Home_team"));
        result.Should().Contain(
            new KeyValuePair<string, string?>("b8e75b4c650603779de82d48eabb555b", "Away_team"));
        result.Should().Contain(
            new KeyValuePair<string, string?>("96c5e6da25184b93d0e8b30361a87d53", "Draw"));
    }
}