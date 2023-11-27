using Azure.Security.KeyVault.Secrets;

namespace OddsCollector.Common.KeyVault.Client;

public class KeyVaultClient(SecretClient? client) : IKeyVaultClient
{
    private const string ApiKeyName = "OddsApiKey";
    private readonly SecretClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public async Task<string> GetOddsApiKey()
    {
        var response = await _client.GetSecretAsync(ApiKeyName).ConfigureAwait(false);

        if (response?.Value is null)
        {
            throw new KeyVaultException(ApiKeyName, $"Failed to fetch {ApiKeyName} key from KeyVault");
        }

        if (string.IsNullOrEmpty(response.Value.Value))
        {
            throw new KeyVaultException(ApiKeyName, $"Secret {ApiKeyName} is null or empty");
        }

        return response.Value.Value;
    }
}
