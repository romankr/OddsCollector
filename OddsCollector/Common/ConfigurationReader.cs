namespace OddsCollector.Common;

public static class ConfigurationReader
{
    public static IEnumerable<string> GetLeagues(IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        return configuration.GetSection("OddsApi:Leagues").GetChildren().Select(c => c.Value);
    }

    public static bool GetGenerateCsv(IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        return configuration.GetValue<bool>("Csv:GenerateCsv");
    }

    public static bool GetGenerateGoogleSheets(IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        return configuration.GetValue<bool>("GoogleApi:GenerateGoogleSheets");
    }
}