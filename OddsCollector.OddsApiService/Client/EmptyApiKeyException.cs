namespace OddsCollector.OddsApiService.Client;

using System;

internal class EmptyApiKeyException : Exception
{
    public EmptyApiKeyException()
    {
    }

    public EmptyApiKeyException(string? message)
        : base(message)
    {
    }

    public EmptyApiKeyException(string? message, Exception? inner)
        : base(message, inner)
    {
    }
}