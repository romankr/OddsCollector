using System.Text.Json.Serialization;

namespace OddsCollector.Functions.Models;

// Stored and returned by name ("HomeTeam", "Draw", "AwayTeam"), so documents stay readable and keep the format
// written before this was an enum.
[JsonConverter(typeof(JsonStringEnumConverter<OutcomeType>))]
internal enum OutcomeType
{
    HomeTeam,
    Draw,
    AwayTeam
}
