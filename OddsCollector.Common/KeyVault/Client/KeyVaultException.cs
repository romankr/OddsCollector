namespace OddsCollector.Common.KeyVault.Client;

public class KeyVaultException : Exception
{
    public KeyVaultException()
    {
        KeyName = null;
    }

    public KeyVaultException(string? keyName)
    {
        KeyName = keyName;
    }

    public KeyVaultException(string? keyName, string? message)
        : base(message)
    {
        KeyName = keyName;
    }

    public KeyVaultException(string? message, Exception? inner)
        : base(message, inner)
    {
        KeyName = null;
    }

    public KeyVaultException(string? keyName, string? message, Exception? inner)
        : base(message, inner)
    {
        KeyName = keyName;
    }

    public string? KeyName { get; }
}
