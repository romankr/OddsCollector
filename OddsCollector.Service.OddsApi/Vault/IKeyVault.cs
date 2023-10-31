namespace OddsCollector.Service.OddsApi.Vault;

internal interface IKeyVault
{
    Task<string> GetOddsApiKey(CancellationToken token);
}
