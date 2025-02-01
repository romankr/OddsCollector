using FunctionApp = OddsCollector.Functions.OddsApi;

namespace OddsCollector.Functions.Tests.Tests.OddsApi.Converters;

internal sealed class ScoreModelConverter
{
    [TestCase("", TestName = "ToEventScore_WithEmptyName_ThrowsException")]
    [TestCase(null, TestName = "ToEventScore_WithNullName_ThrowsException")]
    public void ToEventScore_WithNullOrEmptyName_ThrowsException(string? name)
    {
        var converter = new FunctionApp.Converters.ScoreModelConverter();

        var model = new FunctionApp.WebApi.ScoreModel
        {
            Name = name,
            Score = "1"
        };

        var action = () => converter.ToEventScore(model);

        action.Should().Throw<ArgumentException>().WithParameterName("scoreModel.Name");
    }

    [Test]
    public void ToEventScore_WithInvalidScore_ThrowsException()
    {
        var converter = new FunctionApp.Converters.ScoreModelConverter();

        var model = new FunctionApp.WebApi.ScoreModel
        {
            Name = "name",
            Score = "test"
        };

        var action = () => converter.ToEventScore(model);

        action.Should().Throw<ArgumentException>().WithParameterName("scoreModel")
            .Which.Message.Should().Be("scoreModel must have an integer score. Actual score: test (Parameter 'scoreModel')");
    }
}
