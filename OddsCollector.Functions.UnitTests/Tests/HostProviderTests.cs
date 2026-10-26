namespace OddsCollector.Functions.UnitTests.Tests;

internal sealed class HostProviderTests
{
    [Test]
    public void CreateHost_WhenCalled_ReturnsHost()
    {
        // Act
        var host = HostProvider.CreateHost();

        // Assert
        host.Should().NotBeNull();
    }
}
