using FunctionsApp = OddsCollector.Functions;

namespace OddsCollector.Functions.Tests.Tests;

internal sealed class HostProvider
{
    [Test]
    public void Get_ReturnsHost()
    {
        var host = FunctionsApp.HostProvider.Get();

        host.Should().NotBeNull();
    }
}
