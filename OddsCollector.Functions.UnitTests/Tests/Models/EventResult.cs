using System.Text.Json;
using OddsCollector.Functions.Tests.Infrastructure.Models;
using FunctionApp = OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class EventResult
{
    [TestCase("", TestName = "Init_WithEmptyId_ThrowsArgumentException")]
    [TestCase(" ", TestName = "Init_WithWhitespaceId_ThrowsArgumentException")]
    public void Init_WithBlankId_ThrowsArgumentException(string id)
    {
        var action = () => ValidModels.CreateEventResult() with { Id = id };

        action.Should().ThrowExactly<ArgumentException>().WithParameterName("Id");
    }

    [TestCase("", TestName = "Init_WithEmptyOutcome_ThrowsArgumentException")]
    [TestCase(" ", TestName = "Init_WithWhitespaceOutcome_ThrowsArgumentException")]
    public void Init_WithBlankOutcome_ThrowsArgumentException(string outcome)
    {
        var action = () => ValidModels.CreateEventResult() with { Outcome = outcome };

        action.Should().ThrowExactly<ArgumentException>().WithParameterName("Outcome");
    }

    [TestCase(DateTimeKind.Unspecified, TestName = "Init_WithUnspecifiedCommenceTime_ThrowsArgumentException")]
    [TestCase(DateTimeKind.Local, TestName = "Init_WithLocalCommenceTime_ThrowsArgumentException")]
    public void Init_WithNonUtcCommenceTime_ThrowsArgumentException(DateTimeKind kind)
    {
        var action = () => ValidModels.CreateEventResult() with
        {
            CommenceTime = DateTime.SpecifyKind(ValidModels.CommenceTime, kind)
        };

        action.Should().ThrowExactly<ArgumentException>().WithParameterName("CommenceTime");
    }

    [Test]
    public void Serialize_WithResult_WritesIdInLowercaseForCosmosDb()
    {
        var json = JsonSerializer.Serialize(ValidModels.CreateEventResult());

        using var document = JsonDocument.Parse(json);

        document.RootElement.GetProperty("id").GetString().Should().Be("id");
    }
}
