using Azure;
using Azure.Security.KeyVault.Secrets;

namespace OddsCollector.Common.KeyVault.Client;

public class KeyVaultClient(SecretClient? client) : IKeyVaultClient
{
    private const string ApiKeyName = "OddsApiKey";
    private readonly SecretClient _client = client ?? throw new ArgumentNullException(nameof(client));

    public async Task<string> GetOddsApiKey()
    {
        var response = await _client.GetSecretAsync(ApiKeyName).ConfigureAwait(false);

        return GetSecretValue(response);
    }

    private static string GetSecretValue(Response<KeyVaultSecret> response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return GetValue(response.Value);
    }

    private static string GetValue(KeyVaultSecret secret)
    {
        ArgumentNullException.ThrowIfNull(secret);

        return GetNotEmptyString(secret.Value);
    }

    private static string GetNotEmptyString(string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(value);

        return value;
    }
}
