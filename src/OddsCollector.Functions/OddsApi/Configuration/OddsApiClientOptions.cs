using System.Diagnostics.CodeAnalysis;

namespace OddsCollector.Functions.OddsApi.Configuration;

internal sealed class OddsApiClientOptions
{
    public const string Section = "OddsApiClient";
    public const string LeaguesKey = $"{Section}:Leagues";
    public const string ApiKeyKey = $"{Section}:ApiKey";
    public const string BaseUrlKey = $"{Section}:BaseUrl";

    private static readonly Uri OddsApiUrl = new("https://api.the-odds-api.com");

    public static readonly string DefaultBaseUrl = OddsApiUrl.AbsoluteUri;

    private readonly HashSet<string> _leagues = [];

    [AllowNull]
    public string ApiKey { get; set => field = value ?? string.Empty; } = string.Empty;

    [AllowNull]
    public string BaseUrl
    {
        get;
        set => field = string.IsNullOrWhiteSpace(value) ? DefaultBaseUrl : value.Trim();
    } = DefaultBaseUrl;

    public IReadOnlySet<string> Leagues => _leagues;

    public void AddLeagues(string? leagues)
    {
        if (string.IsNullOrWhiteSpace(leagues))
        {
            return;
        }

        _leagues.UnionWith(leagues.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }
}
