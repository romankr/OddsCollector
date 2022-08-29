namespace OddsCollector.Csv;

using CsvHelper;
using Betting;
using System.Globalization;

public class CsvSaver : ICsvSaver
{
    private static string GetCurrentTimestamp()
    {
        return DateTime.Now.ToString("yyyyddMMHHmmssffff");
    }

    public void WriteBettingStrategyResult(string? dir, string bettingStrategyName, BettingStrategyResult? result)
    {
        if (string.IsNullOrEmpty(dir))
        {
            throw new ArgumentOutOfRangeException(nameof(dir), "dir cannot be null or empty");
        }

        if (string.IsNullOrEmpty(bettingStrategyName))
        {
            throw new ArgumentOutOfRangeException(nameof(bettingStrategyName), "bettingStrategyName cannot be null or empty");
        }

        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        WriteBettingSuggestions(dir, bettingStrategyName, result.Suggestions);
        WriteStatistics(dir, bettingStrategyName, result.Statistics);
    }

    private static void EnsureDirectoryExists(string? dir)
    {
        if (string.IsNullOrEmpty(dir))
        {
            throw new ArgumentOutOfRangeException(nameof(dir), "dir cannot be null or empty");
        }

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    private static void WriteFile<T>(string? filePath, IEnumerable<T>? list)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentOutOfRangeException(nameof(filePath), "filePath cannot be null or empty");
        }

        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(list);
    }

    private static void WriteBettingSuggestions(string? dir, string bettingStrategyName, IEnumerable<BettingSuggestion>? suggestions)
    {
        if (string.IsNullOrEmpty(dir))
        {
            throw new ArgumentOutOfRangeException(nameof(dir), "dir cannot be null or empty");
        }

        if (string.IsNullOrEmpty(bettingStrategyName))
        {
            throw new ArgumentOutOfRangeException(nameof(bettingStrategyName), "bettingStrategyName cannot be null or empty");
        }

        if (suggestions is null)
        {
            throw new ArgumentNullException(nameof(suggestions));
        }
            
        EnsureDirectoryExists(dir);

        var filePath = Path.Combine(dir, $"{bettingStrategyName}_suggestions_{GetCurrentTimestamp()}.csv");

        WriteFile(filePath, suggestions);
    }

    private static void WriteStatistics(string? dir, string bettingStrategyName, Statistics? statistics)
    {
        if (string.IsNullOrEmpty(dir))
        {
            throw new ArgumentOutOfRangeException(nameof(dir), "dir cannot be null or empty");
        }

        if (string.IsNullOrEmpty(bettingStrategyName))
        {
            throw new ArgumentOutOfRangeException(nameof(bettingStrategyName), "bettingStrategyName cannot be null or empty");
        }

        if (statistics is null)
        {
            throw new ArgumentNullException(nameof(statistics));
        }
        
        EnsureDirectoryExists(dir);

        var filePath = Path.Combine(dir, $"{bettingStrategyName}_statistics_{GetCurrentTimestamp()}.csv");

        WriteFile(filePath, new List<Statistics> { statistics });
    }
}