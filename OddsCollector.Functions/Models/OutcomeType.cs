using System.Text.Json.Serialization;

namespace OddsCollector.Functions.Models;

[JsonConverter(typeof(JsonStringEnumConverter<OutcomeType>))]
internal enum OutcomeType
{
    HomeTeam,
    Draw,
    AwayTeam
}
