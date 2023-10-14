﻿namespace OddsCollector.OddsApiService.Jobs;

internal class NotRetrievedException : Exception
{
    public NotRetrievedException()
    {
    }

    public NotRetrievedException(string? message)
        : base(message)
    {
    }

    public NotRetrievedException(string? message, Exception? inner)
        : base(message, inner)
    {
    }
}
