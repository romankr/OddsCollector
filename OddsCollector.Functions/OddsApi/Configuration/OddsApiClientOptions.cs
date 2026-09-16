namespace OddsCollector.Functions.OddsApi.Configuration;

internal sealed class OddsApiClientOptions
{
    public HashSet<string> Leagues { get; init; } = [];

    public string ApiKey { get; set; } = string.Empty;

    public void AddLeagues(string? leagues)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(leagues);

        var deserialized =
            leagues.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        Leagues.UnionWith(deserialized);
    }

    public void SetApiKey(string? apiKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        ApiKey = apiKey;
    }
}
