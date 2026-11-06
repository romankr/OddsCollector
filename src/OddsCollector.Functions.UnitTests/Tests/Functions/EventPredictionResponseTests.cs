using OddsCollector.Functions.Functions;
using OddsCollector.Functions.UnitTests.Infrastructure.Models;

namespace OddsCollector.Functions.UnitTests.Tests.Functions;

internal sealed class EventPredictionResponseTests
{
    [Test]
    public void FromPrediction_WithPrediction_CopiesEveryProperty()
    {
        // Arrange
        var prediction = ValidModels.CreateEventPrediction();

        // Act
        var response = EventPredictionResponse.FromPrediction(prediction);

        // Assert
        response.Should().BeEquivalentTo(prediction);
    }

    [Test]
    public void FromPrediction_WithNullPrediction_Throws()
    {
        // Act
        var action = () => EventPredictionResponse.FromPrediction(null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("prediction");
    }
}
