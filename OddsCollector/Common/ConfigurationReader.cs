namespace OddsCollector.Common
{
    public static class ConfigurationReader
    {
        public static IEnumerable<string> GetLeagues(IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.GetSection("Leagues").GetChildren().Select(c => c.Value);
        }
    }
}
