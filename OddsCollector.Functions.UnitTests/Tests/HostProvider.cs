using FunctionApp = OddsCollector.Functions;

namespace OddsCollector.Functions.Tests.Tests;

internal sealed class HostProvider
{
    [Test]
    public void Get_WhenCalled_ReturnsHost()
    {
        // Act
        var host = FunctionApp.HostProvider.Get();

        // Assert
        host.Should().NotBeNull();
    }
}
