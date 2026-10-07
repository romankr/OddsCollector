namespace OddsCollector.Functions.OddsApi.Configuration;

/// <remarks>
///     Reading the settings never throws. <see cref="OddsApiClientOptionsValidator" /> checks them at startup
///     and reports every problem at once.
/// </remarks>
internal sealed class OddsApiClientOptions
{
    public const string Section = "OddsApiClient";
    public const string LeaguesKey = $"{Section}:Leagues";
    public const string ApiKeyKey = $"{Section}:ApiKey";
    public const string BaseUrlKey = $"{Section}:BaseUrl";

    private static readonly Uri OddsApiUrl = new("https://api.the-odds-api.com");

    public static readonly string DefaultBaseUrl = OddsApiUrl.AbsoluteUri;

    public string BaseUrl { get; set; } = DefaultBaseUrl;

    public HashSet<string> Leagues { get; init; } = [];

    public string ApiKey { get; set; } = string.Empty;

    public void AddLeagues(string? leagues)
    {
        if (string.IsNullOrWhiteSpace(leagues))
        {
            return;
        }

        Leagues.UnionWith(leagues.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    public void SetApiKey(string? apiKey)
    {
        ApiKey = apiKey ?? string.Empty;
    }

    public void SetBaseUrl(string? baseUrl)
    {
        // The setting is optional: production talks to The Odds API, tests point it at a stub.
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return;
        }

        BaseUrl = baseUrl.Trim();
    }
}
