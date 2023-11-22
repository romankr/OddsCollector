using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace OddsCollector.Common.KeyVault.Secrets;

public static class SecretClientFactory
{
    public static SecretClient CreateSecretClient(string? vaultName)
    {
        if (string.IsNullOrEmpty(vaultName))
        {
            throw new ArgumentException($"{nameof(vaultName)} cannot be null or empty", nameof(vaultName));
        }

        return new SecretClient(
            new Uri($"https://{vaultName}.vault.azure.net/"),
            new DefaultAzureCredential(),
            new SecretClientOptions
            {
                Retry =
                {
                    Delay = TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                }
            });
    }
}
