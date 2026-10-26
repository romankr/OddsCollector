using System.Text.Json;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.UnitTests.Infrastructure.Models;

namespace OddsCollector.Functions.UnitTests.Tests.Models;

internal sealed class EventResultTests
{
    [TestCase("", TestName = "Init_WithEmptyId_ThrowsArgumentException")]
    [TestCase(" ", TestName = "Init_WithWhitespaceId_ThrowsArgumentException")]
    public void Init_WithBlankId_ThrowsArgumentException(string id)
    {
        // Arrange
        var eventResult = ValidModels.CreateEventResult();

        // Act
        var action = () => eventResult with { Id = id };

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("Id");
    }

    [Test]
    public void Init_WithUndefinedOutcome_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var eventResult = ValidModels.CreateEventResult();
        const OutcomeType undefinedOutcome = (OutcomeType)42;

        // Act
        var action = () => eventResult with { Outcome = undefinedOutcome };

        // Assert
        action.Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("Outcome");
    }

    [Test]
    public void Serialize_WithResult_WritesOutcomeName()
    {
        // Arrange
        var eventResult = ValidModels.CreateEventResult() with { Outcome = OutcomeType.AwayTeam };

        // Act
        var json = JsonSerializer.Serialize(eventResult);

        // Assert
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("Outcome").GetString().Should().Be("AwayTeam");
    }

    [TestCase(DateTimeKind.Unspecified, TestName = "Init_WithUnspecifiedCommenceTime_ThrowsArgumentException")]
    [TestCase(DateTimeKind.Local, TestName = "Init_WithLocalCommenceTime_ThrowsArgumentException")]
    public void Init_WithNonUtcCommenceTime_ThrowsArgumentException(DateTimeKind kind)
    {
        // Arrange
        var eventResult = ValidModels.CreateEventResult();
        var commenceTime = DateTime.SpecifyKind(ValidModels.CommenceTime, kind);

        // Act
        var action = () => eventResult with { CommenceTime = commenceTime };

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("CommenceTime");
    }

    [Test]
    public void Serialize_WithResult_WritesIdInLowercaseForCosmosDb()
    {
        // Arrange
        var eventResult = ValidModels.CreateEventResult();

        // Act
        var json = JsonSerializer.Serialize(eventResult);

        // Assert
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("id").GetString().Should().Be(eventResult.Id);
    }
}
