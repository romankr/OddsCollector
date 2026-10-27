namespace OddsCollector.Functions.UnitTests.Tests;

internal sealed class HostProviderTests
{
    [Test]
    public void CreateHost_WithoutArguments_ReturnsHost()
    {
        // Act
        using var host = HostProvider.CreateHost([]);

        // Assert
        host.Should().NotBeNull();
    }
}
