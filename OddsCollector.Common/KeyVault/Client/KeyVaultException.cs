namespace OddsCollector.Common.KeyVault.Client;

public class KeyVaultException : Exception
{
    public KeyVaultException()
    {
    }

    public KeyVaultException(string? message)
        : base(message)
    {
    }

    public KeyVaultException(string? message, Exception? inner)
        : base(message, inner)
    {
    }
}
