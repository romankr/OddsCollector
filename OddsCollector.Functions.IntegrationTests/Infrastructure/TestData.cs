namespace OddsCollector.Functions.IntegrationTests.Infrastructure;

internal static class TestData
{
    public static string NewId(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}";
    }

    public static DateTime KickOffInDays(int days)
    {
        return KickOffInHours(days * 24);
    }

    public static DateTime KickOffInHours(int hours)
    {
        var now = DateTime.UtcNow;

        return new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc).AddHours(hours);
    }
}
