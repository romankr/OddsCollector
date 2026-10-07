using OddsCollector.Functions.OddsApi.WebApi;
using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class ScoreModelConverter
{
    [TestCase("", TestName = "ToEventScore_WithEmptyName_ThrowsArgumentException")]
    [TestCase(null, TestName = "ToEventScore_WithNullName_ThrowsArgumentException")]
    [TestCase(" ", TestName = "ToEventScore_WithWhitespaceName_ThrowsArgumentException")]
    public void ToEventScore_WithNullOrEmptyName_ThrowsArgumentException(string? name)
    {
        // Arrange
        var converter = new FunctionApp.ScoreModelConverter();

        var model = new ScoreModel { Name = name, Score = "1" };

        // Act
        var action = () => converter.ToEventScore(model);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName("Name");
    }

    [Test]
    public void ToEventScore_WithNonNumericScore_ThrowsArgumentException()
    {
        // Arrange
        var converter = new FunctionApp.ScoreModelConverter();

        var model = new ScoreModel { Name = "name", Score = "not a number" };

        // Act
        var action = () => converter.ToEventScore(model);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName("scoreModel");
    }
}
