using System.Runtime.CompilerServices;

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
    public static DateTime Require(DateTime? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (!value.HasValue)
        {
            throw new ArgumentNullException(paramName);
        }

        if (value.Value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException($"{paramName} must be UTC. Actual kind: {value.Value.Kind}", paramName);
        }

        return value.Value;
    }
}
