namespace OddsCollector.Common;

/// <summary>
/// Provides date processing methods.
/// </summary>
public static class DateUtility
{
    /// <summary>
    /// Gets current timestamp as a string.
    /// </summary>
    /// <returns>Current timestamp as a string.</returns>
    public static string GetTimestamp()
    {
        return DateTime.Now.ToString("yyyyddMMHHmmssffff");
    }
}