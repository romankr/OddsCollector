using System.Text.Json;
using OddsCollector.Functions.Tests.Infrastructure.Models;
using FunctionApp = OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Tests.Tests.Models;

internal sealed class EventPrediction
{
    private static IEnumerable<TestCaseData> BlankTextCases()
    {
        yield return Case("AwayTeam", "", () => ValidModels.CreateEventPrediction() with { AwayTeam = "" });
        yield return Case("HomeTeam", " ", () => ValidModels.CreateEventPrediction() with { HomeTeam = " " });
        yield return Case("Id", "", () => ValidModels.CreateEventPrediction() with { Id = "" });
        yield return Case("Outcome", " ", () => ValidModels.CreateEventPrediction() with { Outcome = " " });
    }

    private static TestCaseData Case(string property, string value, Func<FunctionApp.EventPrediction> create)
    {
        return new TestCaseData(property, create)
            .SetName($"Init_WithBlank{property}_ThrowsArgumentException(\"{value}\")");
    }

    [TestCaseSource(nameof(BlankTextCases))]
    public void Init_WithBlankText_ThrowsArgumentException(string property, Func<FunctionApp.EventPrediction> create)
    {
        var action = () => create();

        action.Should().ThrowExactly<ArgumentException>().WithParameterName(property);
    }

    [TestCase(DateTimeKind.Unspecified, TestName = "Init_WithUnspecifiedCommenceTime_ThrowsArgumentException")]
    [TestCase(DateTimeKind.Local, TestName = "Init_WithLocalCommenceTime_ThrowsArgumentException")]
    public void Init_WithNonUtcCommenceTime_ThrowsArgumentException(DateTimeKind kind)
    {
        var action = () => ValidModels.CreateEventPrediction() with
        {
            CommenceTime = DateTime.SpecifyKind(ValidModels.CommenceTime, kind)
        };

        action.Should().ThrowExactly<ArgumentException>().WithParameterName("CommenceTime");
    }

    [Test]
    public void Serialize_WithPrediction_WritesIdInLowercaseForCosmosDb()
    {
        var json = JsonSerializer.Serialize(ValidModels.CreateEventPrediction());

        using var document = JsonDocument.Parse(json);

        document.RootElement.GetProperty("id").GetString().Should().Be("id");
    }

    [Test]
    public void Deserialize_WithSerializedPrediction_ReturnsEqualPrediction()
    {
        var prediction = ValidModels.CreateEventPrediction();

        var json = JsonSerializer.Serialize(prediction);

        var deserialized = JsonSerializer.Deserialize<FunctionApp.EventPrediction>(json);

        deserialized.Should().Be(prediction);
    }

    [Test]
    public void Deserialize_WithMissingProperty_ThrowsJsonException()
    {
        var action = () => JsonSerializer.Deserialize<FunctionApp.EventPrediction>("""{"id":"id"}""");

        action.Should().Throw<JsonException>();
    }
}
