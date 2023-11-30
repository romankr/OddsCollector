namespace OddsCollector.Common.OddsApi.Configuration;

public class OddsApiClientOptions
{
    public HashSet<string> Leagues { get; init; } = [];

    public string ApiKey { get; set; } = string.Empty;

    public void AddLeagues(string? leaguesString)
    {
        ArgumentException.ThrowIfNullOrEmpty(leaguesString);

        foreach (var league in leaguesString.Split(";"))
        {
            if (!string.IsNullOrEmpty(league))
            {
                Leagues.Add(league);
            }
        }
    }

    public void SetApiKey(string? apiKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);

        ApiKey = apiKey;
    }
}
