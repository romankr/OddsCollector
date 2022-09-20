namespace OddsCollector.Test.Api;

using FluentAssertions;
using Mocks;
using Models;
using NUnit.Framework;
using OddsCollector.Api.OddsApi;
using System;
using System.Collections.Generic;
using System.Linq;

[TestFixture]
public class OddsApiObjectConverterTests
{
    private readonly OddsApiObjectConverter _converter = 
        new(TestMocks.GetLoggerMock<OddsApiObjectConverter>().Object);

    [Test]
    public void SportEvents()
    {
        var updateTime = DateTime.Today;

        var data = new List<Anonymous2>
        {
            new()
            {
                Away_team = "AwayTeam",
                Home_team = "HomeTeam",
                Commence_time = updateTime,
                Id = "b8776637057325ea7faf2ab37b6e0bb0",
                Sport_key = "soccer_epl",
                Sport_title = "title",
                Bookmakers = new List<Bookmakers>
                {
                    new()
                    {
                        Title = "title",
                        Key = "bookmaker1",
                        Last_update = updateTime,
                        Markets = new List<Markets2>
                        {
                            new()
                            {
                                Key = Markets2Key.H2h,
                                Outcomes = new List<Outcome>
                                {
                                    new()
                                    {
                                        Name = "AwayTeam",
                                        Points = 1,
                                        Price = 1
                                    },
                                    new()
                                    {
                                        Name = "HomeTeam",
                                        Points = 1,
                                        Price = 2
                                    },
                                    new()
                                    {
                                        Name = "Draw",
                                        Points = 1,
                                        Price = 3
                                    }
                                }
                            }
                        }
                    }
                }
            },
            new()
            {
                Away_team = "AwayTeam",
                Home_team = "HomeTeam",
                Commence_time = updateTime,
                Id = "96c5e6da25184b93d0e8b30361a87d53",
                Sport_key = "soccer_epl2",
                Sport_title = "title",
                Bookmakers = new List<Bookmakers>
                {
                    new()
                    {
                        Title = "title",
                        Key = "bookmaker1",
                        Last_update = updateTime,
                        Markets = new List<Markets2>
                        {
                            new()
                            {
                                Key = Markets2Key.H2h,
                                Outcomes = new List<Outcome>
                                {
                                    new()
                                    {
                                        Name = "AwayTeam",
                                        Points = 1,
                                        Price = 1
                                    },
                                    new()
                                    {
                                        Name = "HomeTeam",
                                        Points = 1,
                                        Price = 2
                                    },
                                    new()
                                    {
                                        Name = "Draw",
                                        Points = 1,
                                        Price = 3
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        var result = 
            _converter.ToSportEvents(new ICollection<Anonymous2>[] { data }).ToList();

        result.Should().HaveCount(2);

        result.First().SportEventId.Should().Be("b8776637057325ea7faf2ab37b6e0bb0");
        result.Last().SportEventId.Should().Be("96c5e6da25184b93d0e8b30361a87d53");
    }

    [Test]
    public void SportEventsNullEvent()
    {
        var action = () => { _ = _converter.ToSportEvents(null!); };
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void SportEvent()
    {
        var updateTime = DateTime.Today;

        var data = new Anonymous2
        {
            Away_team = "AwayTeam",
            Home_team = "HomeTeam",
            Commence_time = updateTime,
            Id = "b8776637057325ea7faf2ab37b6e0bb0",
            Sport_key = "soccer_epl",
            Sport_title = "title",
            Bookmakers = new List<Bookmakers>
            {
                new()
                {
                    Title = "title",
                    Key = "bookmaker1",
                    Last_update = updateTime,
                    Markets = new List<Markets2>
                    {
                        new ()
                        {
                            Key = Markets2Key.H2h,
                            Outcomes = new List<Outcome>
                            {
                                new()
                                {
                                    Name = "AwayTeam",
                                    Points = 1,
                                    Price = 1
                                },
                                new()
                                {
                                    Name = "HomeTeam",
                                    Points = 1,
                                    Price = 2
                                },
                                new()
                                {
                                    Name = "Draw",
                                    Points = 1,
                                    Price = 3
                                }
                            }
                        }
                    }
                }
            }
        };

        var result = _converter.ToSportEvent(data);

        result.AwayTeam.Should().Be("AwayTeam");
        result.HomeTeam.Should().Be("HomeTeam");
        result.LeagueId.Should().Be("soccer_epl");
        result.SportEventId.Should().Be("b8776637057325ea7faf2ab37b6e0bb0");
        result.CommenceTime.Should().Be(updateTime);
        result.Outcome.Should().BeNull();
    }

    [Test]
    public void SportEventNullEvent()
    {
        var action = () => { _ = _converter.ToSportEvent(null!); };
        action.Should().Throw<ArgumentNullException>();
    }
    
    [Test]
    public void TestOdds()
    {
        var updateTime = DateTime.Today;

        var bookmakers = new List<Bookmakers>
        {
            new()
            {
                Title = "title",
                Key = "bookmaker1",
                Last_update = updateTime,
                Markets = new List<Markets2>
                {
                    new ()
                    {
                        Key = Markets2Key.H2h,
                        Outcomes = new List<Outcome>
                        {
                            new()
                            {
                                Name = "AwayTeam",
                                Points = 1,
                                Price = 1
                            },
                            new()
                            {
                                Name = "HomeTeam",
                                Points = 1,
                                Price = 2
                            },
                            new()
                            {
                                Name = "Draw",
                                Points = 1,
                                Price = 3
                            }
                        }
                    }
                }
            },
            new()
            {
                Title = "title",
                Key = "bookmaker2",
                Last_update = updateTime,
                Markets = new List<Markets2>
                {
                    new ()
                    {
                        Key = Markets2Key.H2h,
                        Outcomes = new List<Outcome>
                        {
                            new()
                            {
                                Name = "AwayTeam",
                                Points = 1,
                                Price = 1
                            },
                            new()
                            {
                                Name = "HomeTeam",
                                Points = 1,
                                Price = 2
                            },
                            new()
                            {
                                Name = "Draw",
                                Points = 1,
                                Price = 3
                            }
                        }
                    }
                }
            },
            new()
            {
                Title = "title",
                Key = "bookmaker3",
                Last_update = updateTime,
                Markets = new List<Markets2>
                {
                    new ()
                    {
                        Key = Markets2Key.H2h,
                        Outcomes = new List<Outcome>()
                    }
                }
            }
        };

        var sportEvent = new SportEvent
        {
            AwayTeam = "AwayTeam",
            HomeTeam = "HomeTeam",
            SportEventId = "b8776637057325ea7faf2ab37b6e0bb0"
        };

        var result = _converter.ToOdds(bookmakers, sportEvent).ToList();

        result.Should().HaveCount(2);

        var item1 = result.First();

        item1.Bookmaker.Should().Be("bookmaker1");

        var item2 = result.Last();

        item2.Bookmaker.Should().Be("bookmaker2");
    }

    [Test]
    public void TestOddsNullBookmakers()
    {
        var action = () => { _ = _converter.ToOdds(null!, new SportEvent()); };
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TestOddsNullEvent()
    {
        var action = () => { _ = _converter.ToOdds(new List<Bookmakers>(), null!); };
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TestOdd()
    {
        var updateTime = DateTime.Today;

        var testBookmaker = new Bookmakers
        {
            Title = "title",
            Key = "bookmaker",
            Last_update = updateTime,
            Markets = new List<Markets2>
            {
                new ()
                {
                    Key = Markets2Key.H2h,
                    Outcomes = new List<Outcome>
                    {
                        new()
                        {
                            Name = "AwayTeam",
                            Points = 1,
                            Price = 1
                        },
                        new()
                        {
                            Name = "HomeTeam",
                            Points = 1,
                            Price = 2
                        },
                        new()
                        {
                            Name = "Draw",
                            Points = 1,
                            Price = 3
                        }
                    }
                }
            }
        };

        var sportEvent = new SportEvent
        {
            AwayTeam = "AwayTeam",
            HomeTeam = "HomeTeam",
            SportEventId = "b8776637057325ea7faf2ab37b6e0bb0"
        };

        var result = _converter.ToOdd(testBookmaker, sportEvent);

        result.SportEventId.Should().Be("b8776637057325ea7faf2ab37b6e0bb0");
        result.Bookmaker.Should().Be("bookmaker");
        result.DrawOdd.Should().Be(3);
        result.HomeOdd.Should().Be(2);
        result.AwayOdd.Should().Be(1);
        result.LastUpdate.Should().Be(updateTime);
    }

    [Test]
    public void TestOddWithoutDraw()
    {
        var updateTime = DateTime.Today;

        var testBookmaker = new Bookmakers
        {
            Title = "title",
            Key = "bookmaker",
            Last_update = updateTime,
            Markets = new List<Markets2>
            {
                new ()
                {
                    Key = Markets2Key.H2h,
                    Outcomes = new List<Outcome>
                    {
                        new()
                        {
                            Name = "AwayTeam",
                            Points = 1,
                            Price = 1
                        },
                        new()
                        {
                            Name = "HomeTeam",
                            Points = 1,
                            Price = 2
                        }
                    }
                }
            }
        };

        var sportEvent = new SportEvent
        {
            AwayTeam = "AwayTeam",
            HomeTeam = "HomeTeam",
            SportEventId = "b8776637057325ea7faf2ab37b6e0bb0"
        };

        var action = () => { _ = _converter.ToOdd(testBookmaker, sportEvent); };
        action.Should().Throw<KeyNotFoundException>();
    }

    [Test]
    public void TestOddWithoutHomeTeam()
    {
        var updateTime = DateTime.Today;

        var testBookmaker = new Bookmakers
        {
            Title = "title",
            Key = "bookmaker",
            Last_update = updateTime,
            Markets = new List<Markets2>
            {
                new ()
                {
                    Key = Markets2Key.H2h,
                    Outcomes = new List<Outcome>
                    {
                        new()
                        {
                            Name = "AwayTeam",
                            Points = 1,
                            Price = 1
                        },
                        new()
                        {
                            Name = "Draw",
                            Points = 1,
                            Price = 3
                        }
                    }
                }
            }
        };

        var sportEvent = new SportEvent
        {
            AwayTeam = "AwayTeam",
            HomeTeam = "HomeTeam",
            SportEventId = "b8776637057325ea7faf2ab37b6e0bb0"
        };

        var action = () => { _ = _converter.ToOdd(testBookmaker, sportEvent); };
        action.Should().Throw<KeyNotFoundException>();
    }

    [Test]
    public void TestOddWithoutAwayTeam()
    {
        var updateTime = DateTime.Today;

        var testBookmaker = new Bookmakers
        {
            Title = "title",
            Key = "bookmaker",
            Last_update = updateTime,
            Markets = new List<Markets2>
            {
                new ()
                {
                    Key = Markets2Key.H2h,
                    Outcomes = new List<Outcome>
                    {
                        new()
                        {
                            Name = "HomeTeam",
                            Points = 1,
                            Price = 2
                        },
                        new()
                        {
                            Name = "Draw",
                            Points = 1,
                            Price = 3
                        }
                    }
                }
            }
        };

        var sportEvent = new SportEvent
        {
            AwayTeam = "AwayTeam",
            HomeTeam = "HomeTeam",
            SportEventId = "b8776637057325ea7faf2ab37b6e0bb0"
        };

        var action = () => { _ = _converter.ToOdd(testBookmaker, sportEvent); };
        action.Should().Throw<KeyNotFoundException>();
    }

    [Test]
    public void TestOddSportEventNullAwayTeam()
    {
        var updateTime = DateTime.Today;

        var testBookmaker = new Bookmakers
        {
            Title = "title",
            Key = "bookmaker",
            Last_update = updateTime,
            Markets = new List<Markets2>
            {
                new ()
                {
                    Key = Markets2Key.H2h,
                    Outcomes = new List<Outcome>
                    {
                        new()
                        {
                            Name = "AwayTeam",
                            Points = 1,
                            Price = 1
                        },
                        new()
                        {
                            Name = "HomeTeam",
                            Points = 1,
                            Price = 2
                        },
                        new()
                        {
                            Name = "Draw",
                            Points = 1,
                            Price = 3
                        }
                    }
                }
            }
        };

        var sportEvent = new SportEvent
        {
            AwayTeam = null,
            HomeTeam = "HomeTeam",
            SportEventId = "b8776637057325ea7faf2ab37b6e0bb0"
        };

        var action = () => { _ = _converter.ToOdd(testBookmaker, sportEvent); };
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TestOddSportEventNullHomeTeam()
    {
        var updateTime = DateTime.Today;

        var testBookmaker = new Bookmakers
        {
            Title = "title",
            Key = "bookmaker",
            Last_update = updateTime,
            Markets = new List<Markets2>
            {
                new ()
                {
                    Key = Markets2Key.H2h,
                    Outcomes = new List<Outcome>
                    {
                        new()
                        {
                            Name = "AwayTeam",
                            Points = 1,
                            Price = 1
                        },
                        new()
                        {
                            Name = "HomeTeam",
                            Points = 1,
                            Price = 2
                        },
                        new()
                        {
                            Name = "Draw",
                            Points = 1,
                            Price = 3
                        }
                    }
                }
            }
        };

        var sportEvent = new SportEvent
        {
            AwayTeam = "AwayTeam",
            SportEventId = "b8776637057325ea7faf2ab37b6e0bb0"
        };

        var action = () => { _ = _converter.ToOdd(testBookmaker, sportEvent); };
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TestOddNullSportEventId()
    {
        var updateTime = DateTime.Today;

        var testBookmaker = new Bookmakers
        {
            Title = "title",
            Key = "bookmaker",
            Last_update = updateTime,
            Markets = new List<Markets2>
            {
                new ()
                {
                    Key = Markets2Key.H2h,
                    Outcomes = new List<Outcome>
                    {
                        new()
                        {
                            Name = "AwayTeam",
                            Points = 1,
                            Price = 1
                        },
                        new()
                        {
                            Name = "HomeTeam",
                            Points = 1,
                            Price = 2
                        },
                        new()
                        {
                            Name = "Draw",
                            Points = 1,
                            Price = 3
                        }
                    }
                }
            }
        };

        var sportEvent = new SportEvent
        {
            AwayTeam = "AwayTeam",
            HomeTeam = "HomeTeam",
            SportEventId = null
        };

        var result = _converter.ToOdd(testBookmaker, sportEvent);

        result.SportEventId.Should().BeNull();
        result.Bookmaker.Should().Be("bookmaker");
        result.DrawOdd.Should().Be(3);
        result.HomeOdd.Should().Be(2);
        result.AwayOdd.Should().Be(1);
        result.LastUpdate.Should().Be(updateTime);
    }

    [Test]
    public void TestOddNullBookmaker()
    {
        var action = () => { _ = _converter.ToOdd(null! , new SportEvent()); };
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TestOddNullEvent()
    {
        var action = () => { _ = _converter.ToOdd(new Bookmakers(), null!); };
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TestOutcomes()
    {
        var testData = new Bookmakers
        {
            Title = "title",
            Key = "bookmaker",
            Last_update = DateTime.Today,
            Markets = new List<Markets2>
            {
                new ()
                {
                    Key = Markets2Key.H2h,
                    Outcomes = new List<Outcome>
                    {
                        new()
                        {
                            Name = "AwayTeam",
                            Points = 1,
                            Price = 1
                        },
                        new()
                        {
                            Name = "HomeTeam",
                            Points = 1,
                            Price = 2
                        },
                        new()
                        {
                            Name = "Draw",
                            Points = 1,
                            Price = 3
                        }
                    }
                }
            }
        };

        var result = _converter.ToOutcomes(testData);

        result.Should().ContainSingle(p => p.Key == "AwayTeam").Which.Value.Should().Be(1);
        result.Should().ContainSingle(p => p.Key == "HomeTeam").Which.Value.Should().Be(2);
        result.Should().ContainSingle(p => p.Key == "Draw").Which.Value.Should().Be(3);
    }

    [Test]
    public void TestOutcomesLessThan3()
    {
        var testData = new Bookmakers
        {
            Title = "title",
            Key = "bookmaker",
            Last_update = DateTime.Today,
            Markets = new List<Markets2>
            {
                new ()
                {
                    Key = Markets2Key.H2h,
                    Outcomes = new List<Outcome>
                    {
                        new()
                        {
                            Name = "AwayTeam",
                            Points = 1,
                            Price = 1
                        },
                        new()
                        {
                            Name = "HomeTeam",
                            Points = 1,
                            Price = 2
                        }
                    }
                }
            }
        };

        var result = _converter.ToOutcomes(testData);

        result.Should().ContainSingle(p => p.Key == "AwayTeam").Which.Value.Should().Be(1);
        result.Should().ContainSingle(p => p.Key == "HomeTeam").Which.Value.Should().Be(2);
    }

    [Test]
    public void TestOutcomesInBookmakerNull1()
    {
        var testData = new Bookmakers
        {
            Title = "title",
            Key = "bookmaker",
            Last_update = DateTime.Today,
            Markets = new List<Markets2>
            {
                new ()
                {
                    Key = Markets2Key.H2h,
                    Outcomes = null
                }
            }
        };

        var action = () => { _ = _converter.ToOutcomes(testData); };
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TestMarketsNull()
    {
        var testData = new Bookmakers
        {
            Title = "title",
            Key = "bookmaker",
            Last_update = DateTime.Today,
            Markets = null
        };

        var action = () => { _ = _converter.ToOutcomes(testData); };
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TestDifferentMarkets()
    {
        var testData = new Bookmakers
        {
            Title = "title",
            Key = "bookmaker",
            Last_update = DateTime.Today,
            Markets = new List<Markets2>
            {
                new ()
                {
                    Key = Markets2Key.Spreads,
                    Outcomes = new List<Outcome>
                    {
                        new()
                        {
                            Name = "AwayTeam",
                            Points = 1,
                            Price = 1
                        },
                        new()
                        {
                            Name = "HomeTeam",
                            Points = 1,
                            Price = 2
                        }
                    }
                }
            }
        };

        var action = () => { _ = _converter.ToOutcomes(testData); };
        action.Should().Throw<InvalidOperationException> ();
    }

    [Test]
    public void TestOutcomesArgumentNull()
    {
        var action = () => { _ = _converter.ToOutcomes(null!); };
        action.Should().Throw<Exception>();
    }

    [Test]
    public void TestNullApiKey()
    {
        var action = () => { _ = new OddsApiObjectConverter(null!); };
        action.Should().Throw<Exception>();
    }
}