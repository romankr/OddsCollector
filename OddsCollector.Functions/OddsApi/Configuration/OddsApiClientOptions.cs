namespace OddsCollector.Functions.OddsApi.Configuration;

internal sealed class OddsApiClientOptions
{
    public static readonly Uri DefaultBaseUrl = new("https://api.the-odds-api.com");

    public Uri BaseUrl { get; set; } = DefaultBaseUrl;

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

    public void SetBaseUrl(string? baseUrl)
    {
        // The setting is optional: production talks to The Odds API, tests point it at a stub.
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return;
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException($"{nameof(baseUrl)} must be an absolute HTTP(S) URL. Actual value: {baseUrl}",
                nameof(baseUrl));
        }

        BaseUrl = uri;
    }
}
