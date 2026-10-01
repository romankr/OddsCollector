namespace OddsCollector.Functions.Tests.Infrastructure.Time;

internal sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    public static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public static FixedTimeProvider AtNow { get; } = new(Now);

    public override DateTimeOffset GetUtcNow()
    {
        return utcNow;
    }
}
