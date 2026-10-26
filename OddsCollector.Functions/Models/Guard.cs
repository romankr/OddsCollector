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

    public static TEnum Defined<TEnum>(TEnum value, string name) where TEnum : struct, Enum
    {
        // Any integer can be cast to an enum, and JSON may carry a number instead of a name.
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(name, value, $"{name} must be a defined {typeof(TEnum).Name}");
        }

        return value;
    }

    public static T NotNull<T>(T? value, string name) where T : class
    {
        return value ?? throw new ArgumentNullException(name);
    }
}
