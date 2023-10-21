namespace OddsCollector.Service.OddsApi.Jobs;

public class LeaguesNotSpecifiedException : Exception
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
