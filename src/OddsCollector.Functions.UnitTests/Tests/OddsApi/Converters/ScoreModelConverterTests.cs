using OddsCollector.Functions.OddsApi.Converters;
using OddsCollector.Functions.OddsApi.WebApi;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi.Converters;

internal sealed class ScoreModelConverterTests
{
    [Test]
    public void ToEventScore_WithNameAndIntegerScore_ReturnsEventScore()
    {
        // Arrange
        var converter = new ScoreModelConverter();

        var model = new ScoreModel { Name = "name", Score = "3" };

        // Act
        var conversion = converter.ToEventScore(model);

        // Assert
        conversion.Value.Should().Be(new EventScore { Name = "name", Score = 3 });
    }

    [TestCase("", TestName = "ToEventScore_WithEmptyName_ReturnsRejection")]
    [TestCase(null, TestName = "ToEventScore_WithNullName_ReturnsRejection")]
    [TestCase(" ", TestName = "ToEventScore_WithWhitespaceName_ReturnsRejection")]
    public void ToEventScore_WithNullOrEmptyName_ReturnsRejection(string? name)
    {
        // Arrange
        var converter = new ScoreModelConverter();

        var model = new ScoreModel { Name = name, Score = "1" };

        // Act
        var conversion = converter.ToEventScore(model);

        // Assert
        conversion.Value.Should().Be(new Rejection("a score has no team"));
    }

    [Test]
    public void ToEventScore_WithNonNumericScore_ReturnsRejectionNamingTheScore()
    {
        // Arrange
        var converter = new ScoreModelConverter();

        var model = new ScoreModel { Name = "name", Score = "not a number" };

        // Act
        var conversion = converter.ToEventScore(model);

        // Assert
        conversion.Value.Should().Be(new Rejection("the score of name is not an integer: not a number"));
    }

    [Test]
    public void ToEventScore_WithNullModel_ThrowsArgumentNullException()
    {
        // Arrange
        var converter = new ScoreModelConverter();

        // Act
        var action = () => converter.ToEventScore(null!);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("scoreModel");
    }
}
