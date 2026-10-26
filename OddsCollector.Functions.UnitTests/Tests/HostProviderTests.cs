namespace OddsCollector.Functions.UnitTests.Tests;

internal sealed class HostProviderTests
{
    [Test]
    public void Get_WhenCalled_ReturnsHost()
    {
        // Act
        var host = HostProvider.Get();

        // Assert
        host.Should().NotBeNull();
    }
}
