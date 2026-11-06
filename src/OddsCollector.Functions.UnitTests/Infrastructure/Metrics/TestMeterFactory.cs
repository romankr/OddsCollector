using System.Diagnostics.Metrics;

namespace OddsCollector.Functions.UnitTests.Infrastructure.Metrics;

// Meters are scoped to the factory, so a MetricCollector created with it sees only the measurements of one test,
// even while fixtures run in parallel.
internal sealed class TestMeterFactory : IMeterFactory
{
    private readonly List<Meter> _meters = [];

    public Meter Create(MeterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var meter = new Meter(options.Name, options.Version, options.Tags, scope: this);
        _meters.Add(meter);

        return meter;
    }

    public void Dispose()
    {
        foreach (var meter in _meters)
        {
            meter.Dispose();
        }

        _meters.Clear();
    }
}
