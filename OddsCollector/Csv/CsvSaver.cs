namespace OddsCollector.Csv;

using Betting;
using Common;
using CsvHelper;
using System.Globalization;

public class CsvSaver : ICsvSaver
{
    public async Task WriteBettingStrategyResultAsync(string? dir, string bettingStrategyName, BettingStrategyResult? result)
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

        if (result.Suggestions is null)
        {
            throw new Exception("Suggestions are null");
        }

        await WriteBettingSuggestionsAsync(dir, bettingStrategyName, result.Suggestions.OrderBy(e => e.CommenceTime));
        await WriteStatisticsAsync(dir, bettingStrategyName, result.Statistics);
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

    private static async Task WriteFileAsync<T>(string? filePath, IEnumerable<T>? list)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentOutOfRangeException(nameof(filePath), "filePath cannot be null or empty");
        }

        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        await using var writer = new StreamWriter(filePath);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(list);
    }

    private static async Task WriteBettingSuggestionsAsync(string? dir, string bettingStrategyName, IEnumerable<BettingSuggestion>? suggestions)
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

        var filePath = Path.Combine(dir, $"{bettingStrategyName}_suggestions_{DateUtility.GetTimestamp()}.csv");

        await WriteFileAsync(filePath, suggestions);
    }

    private static async Task WriteStatisticsAsync(string? dir, string bettingStrategyName, Statistics? statistics)
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

        var filePath = Path.Combine(dir, $"{bettingStrategyName}_statistics_{DateUtility.GetTimestamp()}.csv");

        await WriteFileAsync(filePath, new List<Statistics> { statistics });
    }
}