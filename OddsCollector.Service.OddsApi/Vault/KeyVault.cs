using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace OddsCollector.Service.OddsApi.Vault;

internal class KeyVault : IKeyVault
{
    private const string ApiKeyName = "OddsApiKey";
    private const string KeyVaultNameKey = "KeyVault:Name";
    private readonly SecretClient _client;

    public KeyVault(IConfiguration? configuration)
    {
        var options = new SecretClientOptions()
        {
            Retry = {
                Delay= TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential
            }
        };

        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        _client = new SecretClient(
            new Uri($"https://{GetApiKey(configuration)}.vault.azure.net/"),
            new DefaultAzureCredential(),
            options);
    }

    public string GetOddsApiKey()
    {
        var secret = _client.GetSecret(ApiKeyName) ?? throw new EmptyApiKeyException("Got empty response");

        if (string.IsNullOrEmpty(secret.Value.Value)) throw new EmptyApiKeyException("Secret has no value");

        return secret.Value.Value;
    }

    private static string GetApiKey(IConfiguration configuration)
    {
        var key = configuration[KeyVaultNameKey];

        if (string.IsNullOrEmpty(key))
        {
            throw new VaultNameKeyException($"{KeyVaultNameKey} property is null or empty.");
        }

        return key;
    }
}
