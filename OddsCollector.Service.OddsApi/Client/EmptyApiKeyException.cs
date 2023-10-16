﻿namespace OddsCollector.Service.OddsApi.Client;

public class EmptyApiKeyException : Exception
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