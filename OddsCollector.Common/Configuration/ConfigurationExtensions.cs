using Microsoft.Extensions.Configuration;

namespace OddsCollector.Common.Configuration;

public static class ConfigurationExtensions
{
    public static T GetRequiredSection<T>(this IConfiguration? configuration)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var typeName = typeof(T).Name;

        var section = configuration.GetSection(typeName);

        if (section is null)
        {
            throw new ConfigurationException($"Configuration section {typeName} is not found.");
        }

        return section.Get<T>() ?? throw new ConfigurationException($"Failed to get {typeName} from configuration.");
    }
}
