namespace OddsCollector.Common;

/// <summary>
/// Reads values form configuration files.
/// </summary>
public static class ConfigurationReader
{
    /// <summary>
    /// Gets list of leagues which odds are going to be collected.
    /// </summary>
    /// <param name="configuration">An <see cref="IConfiguration"/> instance created by the dependency injection container.</param>
    /// <returns>A list of leagues.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    public static IEnumerable<string> GetLeagues(IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        return configuration.GetSection("OddsApi:Leagues").GetChildren().Select(c => c.Value);
    }

    /// <summary>
    /// Gets a flag indicating whether to generate CSV files or not.
    /// </summary>
    /// <param name="configuration">An <see cref="IConfiguration"/> instance created by the dependency injection container.</param>
    /// <returns>A flag indicating whether to generate CSV files or not.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    public static bool GetGenerateCsv(IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        return configuration.GetValue<bool>("Csv:GenerateCsv");
    }

    /// <summary>
    /// Gets a flag indicating whether to generate Google Sheet documents or not.
    /// </summary>
    /// <param name="configuration">An <see cref="IConfiguration"/> instance created by the dependency injection container.</param>
    /// <returns>A flag indicating whether to generate Google Sheet documents or not.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is null.</exception>
    public static bool GetGenerateGoogleSheets(IConfiguration configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        return configuration.GetValue<bool>("GoogleApi:GenerateGoogleSheets");
    }
}