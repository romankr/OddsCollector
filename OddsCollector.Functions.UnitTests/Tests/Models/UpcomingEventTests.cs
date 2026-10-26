using System.Text.Json;
using OddsCollector.Functions.Models;
using OddsCollector.Functions.UnitTests.Infrastructure.Models;

namespace OddsCollector.Functions.UnitTests.Tests.Models;

internal sealed class UpcomingEventTests
{
    private static IEnumerable<TestCaseData> BlankTextCases()
    {
        yield return Case("AwayTeam", "", () => ValidModels.CreateUpcomingEvent() with { AwayTeam = "" });
        yield return Case("HomeTeam", " ", () => ValidModels.CreateUpcomingEvent() with { HomeTeam = " " });
        yield return Case("Id", "", () => ValidModels.CreateUpcomingEvent() with { Id = "" });
    }

    private static TestCaseData Case(string property, string value, Func<UpcomingEvent> create)
    {
        return new TestCaseData(property, create)
            .SetName($"Init_WithBlank{property}_ThrowsArgumentException(\"{value}\")");
    }

    [TestCaseSource(nameof(BlankTextCases))]
    public void Init_WithBlankText_ThrowsArgumentException(string property, Func<UpcomingEvent> create)
    {
        // Act
        var action = () => create();

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName(property);
    }

    [TestCase(DateTimeKind.Unspecified, TestName = "Init_WithUnspecifiedCommenceTime_ThrowsArgumentException")]
    [TestCase(DateTimeKind.Local, TestName = "Init_WithLocalCommenceTime_ThrowsArgumentException")]
    public void Init_WithNonUtcCommenceTime_ThrowsArgumentException(DateTimeKind kind)
    {
        // Arrange
        var upcomingEvent = ValidModels.CreateUpcomingEvent();
        var commenceTime = DateTime.SpecifyKind(ValidModels.CommenceTime, kind);

        // Act
        var action = () => upcomingEvent with { CommenceTime = commenceTime };

        // Assert
        action.Should().ThrowExactly<ArgumentException>().WithParameterName("CommenceTime");
    }

    [Test]
    public void Init_WithNullOdds_ThrowsArgumentNullException()
    {
        // Arrange
        var upcomingEvent = ValidModels.CreateUpcomingEvent();

        // Act
        var action = () => upcomingEvent with { Odds = null! };

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("Odds");
    }

    [Test]
    public void Deserialize_WithSerializedEvent_ReturnsEquivalentEvent()
    {
        // Arrange
        var upcomingEvent = ValidModels.CreateUpcomingEvent();
        var json = JsonSerializer.Serialize(upcomingEvent);

        // Act
        var deserialized = JsonSerializer.Deserialize<UpcomingEvent>(json);

        // Assert
        deserialized.Should().BeEquivalentTo(upcomingEvent);
    }
}
