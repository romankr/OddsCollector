using FluentAssertions;
using OddsCollector.Common.KeyVault.Secrets;

namespace OddsCollector.Common.Tests.KeyVault.Secrets;

[Parallelizable(ParallelScope.All)]
internal sealed class SecretClientFactoryTests
{
    [Test]
    public void CreateSecretClient_WithValidName_ReturnsNewInstance()
    {
        const string vaultName = "vaultName";
        var expectedUri = new Uri($"https://{vaultName}.vault.azure.net/");

        var result = SecretClientFactory.CreateSecretClient("vaultName");

        result.Should().NotBeNull();
        result.VaultUri.Should().NotBeNull().And.Be(expectedUri);
    }

    [TestCase("")]
    [TestCase(null)]
    public void CreateSecretClient_WithNullOrEmptyString_ThrowsException(string? vaultName)
    {
        var action = () =>
        {
            _ = SecretClientFactory.CreateSecretClient(vaultName);
        };

        action.Should().Throw<ArgumentException>().WithParameterName(nameof(vaultName));
    }
}
