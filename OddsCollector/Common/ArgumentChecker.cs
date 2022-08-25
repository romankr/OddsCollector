namespace OddsCollector.Common;

/// <summary>
/// Checks function arguments.
/// </summary>
public class ArgumentChecker
{
    /// <summary>
    /// Checks if provided parameter is null.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="parameterName">The parameter's name.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is null.</exception>
    public static void NullCheck(object? parameter, string parameterName)
    {
        if (parameter is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }

    /// <summary>
    /// Checks if provided parameter is not null or empty.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="parameterName">The parameter's name.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="parameterName"/> cannot be null or empty</exception>
    public static void NullOrEmptyCheck(string? parameter, string parameterName)
    {
        if (string.IsNullOrEmpty(parameter))
        {
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} cannot be null or empty");
        }
    }
}