namespace OddsCollector.Functions.OddsApi.Configuration;

internal sealed class OddsApiClientOptions
{
    public const string Section = "OddsApiClient";
    public const string LeaguesKey = $"{Section}:Leagues";
    public const string ApiKeyKey = $"{Section}:ApiKey";
    public const string BaseUrlKey = $"{Section}:BaseUrl";

    private static readonly Uri OddsApiUrl = new("https://api.the-odds-api.com");

    public static readonly string DefaultBaseUrl = OddsApiUrl.AbsoluteUri;

    public string BaseUrl { get; set; } = DefaultBaseUrl;

    private readonly HashSet<string> _leagues = [];

    public IReadOnlySet<string> Leagues => _leagues;

    public string ApiKey { get; set; } = string.Empty;

    public void AddLeagues(string? leagues)
    {
        if (string.IsNullOrWhiteSpace(leagues))
        {
            return;
        }

        _leagues.UnionWith(leagues.Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    public void SetApiKey(string? apiKey)
    {
        ApiKey = apiKey ?? string.Empty;
    }

    public void SetBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return;
        }

        BaseUrl = baseUrl.Trim();
    }
}
