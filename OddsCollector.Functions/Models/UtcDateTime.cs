namespace OddsCollector.Functions.Models;

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
