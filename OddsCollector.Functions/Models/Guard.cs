namespace OddsCollector.Functions.Models;

/// <summary>
///     Checks the models run from their init accessors, so a model is checked the same way however it is
///     created: in code, from a Service Bus message or from a Cosmos DB document.
/// </summary>
internal static class Guard
{
    public static string NotBlank(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} cannot be empty", name);
        }

        return value;
    }

    public static T NotNull<T>(T? value, string name) where T : class
    {
        return value ?? throw new ArgumentNullException(name);
    }
}
