using OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.UnitTests.Tests.OddsApi.Converters;

internal sealed class ScoreModelsConverterTests
{
    [Test]
    public void Convert_WithNullScores_ThrowsArgumentNullException()
    {
        // Arrange
        var converter = new ScoreModelsConverter(Substitute.For<IScoreModelConverter>());

        // Act
        var action = () => converter.Convert(null);

        // Assert
        action.Should().Throw<ArgumentNullException>().WithParameterName("scores");
    }

    [Test]
    public void Convert_WithoutTwoScores_ThrowsArgumentException()
    {
        // Arrange
        var converter = new ScoreModelsConverter(Substitute.For<IScoreModelConverter>());

        // Act
        var action = () => converter.Convert([]);

        // Assert
        action.Should().Throw<ArgumentException>().WithParameterName("scores")
            .Which.Message.Should().Be("scores must have 2 elements (Parameter 'scores')");
    }
}
