namespace OddsCollector.Common.KeyVault.Client;

public interface IKeyVaultClient
{
    Task<string> GetOddsApiKey();
}
