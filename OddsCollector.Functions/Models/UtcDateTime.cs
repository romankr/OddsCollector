namespace OddsCollector.Functions.Models;

/// <summary>
///     Every point in time the app stores or compares is UTC.
/// </summary>
/// <remarks>
///     PredictionsHttpFunction compares stored times with GetCurrentDateTime() as ISO 8601 strings, and the
///     "already started" checks compare them with TimeProvider.GetUtcNow(). DateTime comparison ignores Kind,
///     so a local or unspecified value would be off by the machine's offset without any error.
/// </remarks>
internal static class UtcDateTime
{
    public static DateTime Require(DateTime value, string name)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException($"{name} must be UTC. Actual kind: {value.Kind}", name);
        }

        return value;
    }
}
