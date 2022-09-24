namespace OddsCollector.Common;

/// <summary>
/// Checks values.
/// </summary>
public class ValueChecker
{
    /// <summary>
    /// Checks if provided parameter is not null or empty.
    /// </summary>
    /// <param name="value">The parameter.</param>
    /// <param name="parameterName">The parameter's name.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="parameterName"/> cannot be null or empty</exception>
    public static void NullOrEmptyCheck(string? value, string parameterName)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new Exception($"{parameterName} cannot be null or empty");
        }
    }
}