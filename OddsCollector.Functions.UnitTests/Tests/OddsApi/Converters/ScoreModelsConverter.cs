using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class ScoreModelsConverter
{
    [Test]
    public void Convert_WithNullScores_ThrowsArgumentNullException()
    {
        // Arrange
        var converter = new FunctionApp.ScoreModelsConverter(Substitute.For<FunctionApp.IScoreModelConverter>());

        // Act
        var action = () => converter.Convert(null);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("scores");
    }

    [Test]
    public void Convert_WithoutTwoScores_ThrowsArgumentException()
    {
        // Arrange
        var converter = new FunctionApp.ScoreModelsConverter(Substitute.For<FunctionApp.IScoreModelConverter>());

        // Act
        var action = () => converter.Convert([]);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName("scores")
            .Which.Message.Should().Be("scores must have 2 elements (Parameter 'scores')");
    }
}
