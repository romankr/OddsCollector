namespace OddsCollector.Functions.Tests.Tests;

internal sealed class HostProvider
{
    [Test]
    public void Get_ReturnsHost()
    {
        var host = OddsCollector.Functions.HostProvider.Get();

        host.Should().NotBeNull();
    }
}
