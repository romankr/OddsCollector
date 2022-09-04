namespace OddsCollector.Csv;

using Betting;
using Common;
using CsvHelper;
using System.Globalization;

/// <summary>
/// Writes betting strategy suggestions and effectiveness to CSV files.
/// </summary>
public class CsvSaver : ICsvSaver
{
    private readonly string _outputPath;
    private readonly bool _enabled;

    /// <summary>
    /// A constructor that is suitable for the dependency injection.
    /// </summary>
    /// <param name="config">An <see cref="IConfiguration"/> instance created by the dependency injection container.</param>
    /// <exception cref="ArgumentNullException"><paramref name="config"/> is null.</exception>
    /// <exception cref="Exception">CSV output path cannot be null or empty.</exception>
    public CsvSaver(IConfiguration config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        _enabled = ConfigurationReader.GetGenerateCsv(config);
        _outputPath = config.GetValue<string>("Csv:OutputPath");

        if (!_enabled)
        {
            // skipping parameter validation.
            return;
        }
        
        if (string.IsNullOrEmpty(_outputPath))
        {
            throw new Exception("CSV output path cannot be null or empty.");
        }
    }

    /// <summary>
    /// Write betting strategy suggestions and effectiveness to CSV files.
    /// </summary>
    /// <param name="bettingStrategyName">A betting strategy name.</param>
    /// <param name="result">Betting strategy suggestions and effectiveness in <see cref="BettingStrategyResult"/>.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bettingStrategyName"/> cannot be null or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is null.</exception>
    /// <exception cref="Exception">
    /// Suggestions are null or
    /// Csv generation is disabled.
    /// </exception>
    public async Task WriteBettingStrategyResultAsync(string bettingStrategyName, BettingStrategyResult? result)
    {
        if (string.IsNullOrEmpty(bettingStrategyName))
        {
            throw new ArgumentOutOfRangeException(nameof(bettingStrategyName), 
                "bettingStrategyName cannot be null or empty.");
        }

        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (result.Suggestions is null)
        {
            throw new Exception("Suggestions are null.");
        }

        if (!_enabled)
        {
            throw new Exception("Csv generation is disabled.");
        }

        await WriteBettingSuggestionsAsync(
            bettingStrategyName, 
            result.Suggestions.OrderBy(e => e.CommenceTime));
        
        await WriteStatisticsAsync(bettingStrategyName, result.Statistics);
    }

    /// <summary>
    /// Creates directory if it doesn't exist.
    /// </summary>
    /// <param name="dir">Directory path.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dir"/> cannot be null or empty.</exception>
    private static void EnsureDirectoryExists(string? dir)
    {
        if (string.IsNullOrEmpty(dir))
        {
            throw new ArgumentOutOfRangeException(nameof(dir), "dir cannot be null or empty.");
        }

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    /// <summary>
    /// Writes an object list to a file.
    /// </summary>
    /// <typeparam name="T">Type of the objects.</typeparam>
    /// <param name="filePath">Full path to the file.</param>
    /// <param name="list">Object list.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="filePath"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
    private static async Task WriteFileAsync<T>(string? filePath, IEnumerable<T>? list)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentOutOfRangeException(nameof(filePath), 
                "filePath cannot be null or empty.");
        }

        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        await using var writer = new StreamWriter(filePath);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(list);
    }

    /// <summary>
    /// Writes betting suggestions to a file.
    /// </summary>
    /// <param name="bettingStrategyName">Betting strategy name.</param>
    /// <param name="suggestions">A list of <see cref="BettingSuggestion"/>.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bettingStrategyName"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="suggestions"/> is null.</exception>
    private async Task WriteBettingSuggestionsAsync(string bettingStrategyName, IEnumerable<BettingSuggestion>? suggestions)
    {
        if (string.IsNullOrEmpty(bettingStrategyName))
        {
            throw new ArgumentOutOfRangeException(nameof(bettingStrategyName), 
                "bettingStrategyName cannot be null or empty.");
        }

        if (suggestions is null)
        {
            throw new ArgumentNullException(nameof(suggestions));
        }
            
        EnsureDirectoryExists(_outputPath);

        var filePath = Path.Combine(_outputPath, $"{bettingStrategyName}_suggestions_{DateUtility.GetTimestamp()}.csv");

        await WriteFileAsync(filePath, suggestions);
    }

    /// <summary>
    /// Writes betting strategy effectiveness to a file.
    /// </summary>
    /// <param name="bettingStrategyName">Betting strategy name.</param>
    /// <param name="statistics">Betting strategy effectiveness <see cref="Statistics"/>.</param>
    /// <returns>A <see cref="Task"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bettingStrategyName"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="statistics"/> is null.</exception>
    private async Task WriteStatisticsAsync(string bettingStrategyName, Statistics? statistics)
    {
        if (string.IsNullOrEmpty(bettingStrategyName))
        {
            throw new ArgumentOutOfRangeException(nameof(bettingStrategyName), 
                "bettingStrategyName cannot be null or empty.");
        }

        if (statistics is null)
        {
            throw new ArgumentNullException(nameof(statistics));
        }
        
        EnsureDirectoryExists(_outputPath);

        var filePath = Path.Combine(_outputPath, $"{bettingStrategyName}_statistics_{DateUtility.GetTimestamp()}.csv");

        await WriteFileAsync(filePath, new List<Statistics> { statistics });
    }
}