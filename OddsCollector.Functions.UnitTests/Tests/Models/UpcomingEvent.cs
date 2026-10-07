using System.Text.Json;
using OddsCollector.Functions.Tests.Infrastructure.Models;
using FunctionApp = OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class UpcomingEvent
{
    private static IEnumerable<TestCaseData> BlankTextCases()
    {
        yield return Case("AwayTeam", "", () => ValidModels.CreateUpcomingEvent() with { AwayTeam = "" });
        yield return Case("HomeTeam", " ", () => ValidModels.CreateUpcomingEvent() with { HomeTeam = " " });
        yield return Case("Id", "", () => ValidModels.CreateUpcomingEvent() with { Id = "" });
    }

    private static TestCaseData Case(string property, string value, Func<FunctionApp.UpcomingEvent> create)
    {
        return new TestCaseData(property, create)
            .SetName($"Init_WithBlank{property}_ThrowsArgumentException(\"{value}\")");
    }

    [TestCaseSource(nameof(BlankTextCases))]
    public void Init_WithBlankText_ThrowsArgumentException(string property, Func<FunctionApp.UpcomingEvent> create)
    {
        var action = () => create();

        action.Should().ThrowExactly<ArgumentException>().WithParameterName(property);
    }

    [TestCase(DateTimeKind.Unspecified, TestName = "Init_WithUnspecifiedCommenceTime_ThrowsArgumentException")]
    [TestCase(DateTimeKind.Local, TestName = "Init_WithLocalCommenceTime_ThrowsArgumentException")]
    public void Init_WithNonUtcCommenceTime_ThrowsArgumentException(DateTimeKind kind)
    {
        var action = () => ValidModels.CreateUpcomingEvent() with
        {
            CommenceTime = DateTime.SpecifyKind(ValidModels.CommenceTime, kind)
        };

        action.Should().ThrowExactly<ArgumentException>().WithParameterName("CommenceTime");
    }

    [Test]
    public void Init_WithNullOdds_ThrowsArgumentNullException()
    {
        var action = () => ValidModels.CreateUpcomingEvent() with { Odds = null! };

        action.Should().ThrowExactly<ArgumentNullException>().WithParameterName("Odds");
    }

    [Test]
    public void Deserialize_WithSerializedEvent_ReturnsEquivalentEvent()
    {
        var upcomingEvent = ValidModels.CreateUpcomingEvent();

        var json = JsonSerializer.Serialize(upcomingEvent);

        var deserialized = JsonSerializer.Deserialize<FunctionApp.UpcomingEvent>(json);

        deserialized.Should().BeEquivalentTo(upcomingEvent);
    }
}
