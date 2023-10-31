using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using OddsCollector.Common.Configuration;

namespace OddsCollector.Service.OddsApi.Vault;

internal sealed class KeyVault : IKeyVault
{
    private const string ApiKeyName = "OddsApiKey";
    private readonly SecretClient _client;

    public KeyVault(IConfiguration? configuration)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var options = new SecretClientOptions
        {
            Retry =
            {
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential
            }
        };

        var vault = configuration.GetRequiredSection<KeyVaultOptions>().Name;

        _client = new SecretClient(
            new Uri($"https://{vault}.vault.azure.net/"),
            new DefaultAzureCredential(),
            options);
    }

    public async Task<string> GetOddsApiKey(CancellationToken token)
    {
        var secret = await _client.GetSecretAsync(ApiKeyName, cancellationToken: token).ConfigureAwait(false) ??
                     throw new VaultException($"Failed to fetch {ApiKeyName} key from vault");

        if (string.IsNullOrEmpty(secret.Value.Value))
        {
            throw new VaultException($"{ApiKeyName} is null or empty");
        }

        return secret.Value.Value;
    }
}
