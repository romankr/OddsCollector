using System.Text.Json;
using System.Text.Json.Serialization;

namespace OddsCollector.Functions.Models;

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
