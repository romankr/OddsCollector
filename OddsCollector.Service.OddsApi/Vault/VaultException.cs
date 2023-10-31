namespace OddsCollector.Service.OddsApi.Vault;

public class VaultException : Exception
{
    public VaultException()
    {
    }

    public VaultException(string? message)
        : base(message)
    {
    }

    public VaultException(string? message, Exception? inner)
        : base(message, inner)
    {
    }
}
