namespace OddsCollector.Common;

public static class DateUtility
{
    public static string GetTimestamp()
    {
        return DateTime.Now.ToString("yyyyddMMHHmmssffff");
    }
}