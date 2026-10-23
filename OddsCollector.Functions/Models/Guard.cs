namespace OddsCollector.Functions.Models;

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
