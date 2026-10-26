using System.Text.Json;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.UnitTests.Infrastructure.Models;

namespace OddsCollector.Functions.UnitTests.Tests.Models;

internal sealed class EventPredictionTests
{
    private static IEnumerable<TestCaseData> BlankTextCases()
    {
        yield return Case("AwayTeam", "", () => ValidModels.CreateEventPrediction() with { AwayTeam = "" });
        yield return Case("HomeTeam", " ", () => ValidModels.CreateEventPrediction() with { HomeTeam = " " });
        yield return Case("Id", "", () => ValidModels.CreateEventPrediction() with { Id = "" });
    }

    private static TestCaseData Case(string property, string value, Func<EventPrediction> create)
    {
        return new TestCaseData(property, create)
            .SetName($"Init_WithBlank{property}_ThrowsArgumentException(\"{value}\")");
    }

    [TestCaseSource(nameof(BlankTextCases))]
    public void Init_WithBlankText_ThrowsArgumentException(string property, Func<EventPrediction> create)
    {
        // Act
        var action = () => create();

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName(property);
    }

    [Test]
    public void Init_WithUndefinedOutcome_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var prediction = ValidModels.CreateEventPrediction();
        const OutcomeType undefinedOutcome = (OutcomeType)42;

        // Act
        var action = () => prediction with { Outcome = undefinedOutcome };

        // Assert
        action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("Outcome");
    }

    [TestCase(OutcomeType.HomeTeam, "HomeTeam", TestName = "Serialize_WithHomeTeamOutcome_WritesItsName")]
    [TestCase(OutcomeType.Draw, "Draw", TestName = "Serialize_WithDrawOutcome_WritesItsName")]
    [TestCase(OutcomeType.AwayTeam, "AwayTeam", TestName = "Serialize_WithAwayTeamOutcome_WritesItsName")]
    public void Serialize_WithOutcome_WritesItsName(OutcomeType outcome, string expectedName)
    {
        // Arrange: documents written before Outcome became an enum hold these names, so they must not change.
        var prediction = ValidModels.CreateEventPrediction() with { Outcome = outcome };

        // Act
        var json = JsonSerializer.Serialize(prediction);

        // Assert
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("Outcome").GetString().Should().Be(expectedName);
    }

    [Test]
    public void Deserialize_WithUndefinedOutcomeNumber_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var json = JsonSerializer.Serialize(ValidModels.CreateEventPrediction())
            .Replace("\"Outcome\":\"HomeTeam\"", "\"Outcome\":42", StringComparison.Ordinal);

        // Act
        var action = () => JsonSerializer.Deserialize<EventPrediction>(json);

        // Assert
        action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("Outcome");
    }

    [TestCase(DateTimeKind.Unspecified, TestName = "Init_WithUnspecifiedCommenceTime_ThrowsArgumentException")]
    [TestCase(DateTimeKind.Local, TestName = "Init_WithLocalCommenceTime_ThrowsArgumentException")]
    public void Init_WithNonUtcCommenceTime_ThrowsArgumentException(DateTimeKind kind)
    {
        // Arrange
        var prediction = ValidModels.CreateEventPrediction();
        var commenceTime = DateTime.SpecifyKind(ValidModels.CommenceTime, kind);

        // Act
        var action = () => prediction with { CommenceTime = commenceTime };

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("CommenceTime");
    }

    [Test]
    public void Serialize_WithPrediction_WritesIdInLowercaseForCosmosDb()
    {
        // Arrange
        var prediction = ValidModels.CreateEventPrediction();

        // Act
        var json = JsonSerializer.Serialize(prediction);

        // Assert
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("id").GetString().Should().Be(prediction.Id);
    }

    [Test]
    public void Deserialize_WithSerializedPrediction_ReturnsEqualPrediction()
    {
        // Arrange
        var prediction = ValidModels.CreateEventPrediction();
        var json = JsonSerializer.Serialize(prediction);

        // Act
        var deserialized = JsonSerializer.Deserialize<EventPrediction>(json);

        // Assert
        deserialized.Should().Be(prediction);
    }

    [Test]
    public void Deserialize_WithMissingProperty_ThrowsJsonException()
    {
        // Arrange
        const string json = """{"id":"id"}""";

        // Act
        var action = () => JsonSerializer.Deserialize<EventPrediction>(json);

        // Assert
        action.Should().Throw<JsonException>();
    }
}
