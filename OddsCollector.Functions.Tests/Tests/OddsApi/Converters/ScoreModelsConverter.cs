using FunctionApp = OddsCollector.Functions.OddsApi.Converters;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class ScoreModelsConverter
{
    [Test]
    public void Convert_WithNullModels_ThrowsException()
    {
        var converter =
            new FunctionApp.ScoreModelsConverter(
                Substitute.For<FunctionApp.IScoreModelConverter>());

        var action = () => converter.Convert(null).ToList();

        action.Should().Throw<ArgumentNullException>().WithParameterName("scoreModels");
    }

    [Test]
    public void Convert_WithNotEnoughModels_ThrowsException()
    {
        var converter =
            new FunctionApp.ScoreModelsConverter(
                Substitute.For<FunctionApp.IScoreModelConverter>());

        var action = () => converter.Convert([]).ToList();

        action.Should().Throw<ArgumentException>().WithParameterName("scoreModels")
            .Which.Message.Should().Be("scoreModels must have 2 elements (Parameter 'scoreModels')");
    }
}
