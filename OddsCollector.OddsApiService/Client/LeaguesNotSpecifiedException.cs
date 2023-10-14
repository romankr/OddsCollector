namespace OddsCollector.OddsApiService.Client;

using System;

internal class LeaguesNotSpecifiedException : Exception
{
    public LeaguesNotSpecifiedException()
    {
    }

    public LeaguesNotSpecifiedException(string? message)
        : base(message)
    {
    }

    public LeaguesNotSpecifiedException(string? message, Exception? inner)
        : base(message, inner)
    {
    }
}