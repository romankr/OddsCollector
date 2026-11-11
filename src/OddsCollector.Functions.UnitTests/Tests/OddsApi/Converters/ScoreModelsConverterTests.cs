using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi.Converters;

internal sealed class ScoreModelsConverterTests
{
    private static readonly ScoreModel HomeScoreModel = new() { Name = "homeTeam", Score = "2" };
    private static readonly ScoreModel AwayScoreModel = new() { Name = "awayTeam", Score = "1" };

    [Test]
    public void Convert_WithTwoScores_ReturnsBothEventScores()
    {
        // Arrange
        var converter = new ScoreModelsConverter(new ScoreModelConverter());

        // Act
        var conversion = converter.Convert([HomeScoreModel, AwayScoreModel]);

        // Assert
        conversion.Value.Should().BeEquivalentTo(new[]
        {
            new EventScore { Name = "homeTeam", Score = 2 },
            new EventScore { Name = "awayTeam", Score = 1 }
        });
    }

    [Test]
    public void Convert_WithNullScores_ReturnsRejection()
    {
        // Arrange
        var converter = new ScoreModelsConverter(Substitute.For<IScoreModelConverter>());

        // Act
        var conversion = converter.Convert(null);

        // Assert
        conversion.Value.Should().Be(new Rejection("no scores"));
    }

    [Test]
    public void Convert_WithoutTwoScores_ReturnsRejection()
    {
        // Arrange
        var converter = new ScoreModelsConverter(Substitute.For<IScoreModelConverter>());

        // Act
        var conversion = converter.Convert([HomeScoreModel]);

        // Assert
        conversion.Value.Should().Be(new Rejection("expected 2 scores, got 1"));
    }

    [Test]
    public void Convert_WithRejectedScore_ReturnsItsRejection()
    {
        // Arrange
        var expectedRejection = new Rejection("reason");

        Conversion<EventScore> rejected = expectedRejection;

        var scoreModelConverterStub = Substitute.For<IScoreModelConverter>();
        scoreModelConverterStub.ToEventScore(Arg.Any<ScoreModel>()).Returns(rejected);

        var converter = new ScoreModelsConverter(scoreModelConverterStub);

        // Act
        var conversion = converter.Convert([HomeScoreModel, AwayScoreModel]);

        // Assert
        conversion.Value.Should().BeSameAs(expectedRejection);
    }
}
