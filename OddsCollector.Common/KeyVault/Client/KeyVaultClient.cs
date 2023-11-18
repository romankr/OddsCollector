using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;
using OddsCollector.Common.KeyVault.Configuration;

namespace OddsCollector.Common.KeyVault.Client;

public class KeyVaultClient : IKeyVaultClient
{
    private const string ApiKeyName = "OddsApiKey";
    private readonly SecretClient _client;

    public KeyVaultClient(IOptions<KeyVaultOptions> keyVaultOptions)
    {
        if (keyVaultOptions is null)
        {
            throw new ArgumentNullException(nameof(keyVaultOptions));
        }

        var secrectOptions = new SecretClientOptions
        {
            Retry =
            {
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential
            }
        };

        _client = new SecretClient(
            new Uri($"https://{keyVaultOptions.Value.Name}.vault.azure.net/"),
            new DefaultAzureCredential(),
            secrectOptions);
    }

    public async Task<string> GetOddsApiKey()
    {
        var secret = await _client.GetSecretAsync(ApiKeyName).ConfigureAwait(false) ??
                     throw new KeyVaultException($"Failed to fetch {ApiKeyName} key from vault");

        if (string.IsNullOrEmpty(secret.Value.Value))
        {
            throw new KeyVaultException($"{ApiKeyName} is null or empty");
        }

        return secret.Value.Value;
    }
}
