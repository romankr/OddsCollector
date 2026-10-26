namespace OddsCollector.Functions.Functions;

// Retry settings shared by the timer functions that call The Odds API; attribute arguments must be constants.
internal static class TimerRetry
{
    public const int MaxRetryCount = 2;
    public const string MinimumInterval = "00:00:30";
    public const string MaximumInterval = "00:02:00";
}
