namespace OddsCollector.Functions.IntegrationTests.Infrastructure;

/// <summary>
///     Values that tests need to be unique or relative to the current time.
/// </summary>
internal static class TestData
{
    /// <summary>
    ///     An id no other test uses, so tests that share the environment never see each other's documents.
    /// </summary>
    public static string NewId(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}";
    }

    /// <summary>
    ///     A kick-off time relative to now, on the hour, so it survives the round trip through
    ///     The Odds API format (whole seconds) unchanged.
    /// </summary>
    public static DateTime KickOffInDays(int days)
    {
        return KickOffInHours(days * 24);
    }

    /// <inheritdoc cref="KickOffInDays" />
    public static DateTime KickOffInHours(int hours)
    {
        var now = DateTime.UtcNow;

        return new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc).AddHours(hours);
    }
}
