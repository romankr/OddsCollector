namespace OddsCollector.Service.OddsApi.Vault;

public class VaultNameKeyException : Exception
{
    public VaultNameKeyException()
    {
    }

    public VaultNameKeyException(string? message)
        : base(message)
    {
    }

    public VaultNameKeyException(string? message, Exception? inner)
        : base(message, inner)
    {
    }
}
