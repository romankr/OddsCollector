using System.Text.Json;
using System.Text.Json.Serialization;

namespace OddsCollector.Functions.Models;

// Dates are stored in Cosmos DB as Unix seconds, so that queries compare them as numbers
// (for example with GetCurrentTimestamp() / 1000) rather than as strings. Fractions of a second are dropped:
// The Odds API gives kick-off times in whole seconds.
internal sealed class UnixSecondsDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt64(out var seconds))
        {
            throw new JsonException("A date must be a whole number of Unix seconds");
        }

        return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteNumberValue(new DateTimeOffset(value.ToUniversalTime()).ToUnixTimeSeconds());
    }
}
