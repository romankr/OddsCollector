namespace OddsCollector.Csv;

using Betting;
using Common;
using CsvHelper;
using System.Globalization;

public class CsvSaver : ICsvSaver
{
    private readonly string _outputPath;
    private readonly bool _enabled;

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
            return;
        }
        
        if (string.IsNullOrEmpty(_outputPath))
        {
            throw new Exception("CSV output path cannot be null or empty.");
        }
    }
    
    public async Task WriteBettingStrategyResultAsync(string bettingStrategyName, BettingStrategyResult? result)
    {
        if (string.IsNullOrEmpty(bettingStrategyName))
        {
            throw new ArgumentOutOfRangeException(nameof(bettingStrategyName), 
                "bettingStrategyName cannot be null or empty");
        }

        if (result is null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        if (result.Suggestions is null)
        {
            throw new Exception("Suggestions are null");
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
            throw new ArgumentOutOfRangeException(nameof(filePath), 
                "filePath cannot be null or empty");
        }

        if (list is null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        await using var writer = new StreamWriter(filePath);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(list);
    }

    private async Task WriteBettingSuggestionsAsync(string bettingStrategyName, IEnumerable<BettingSuggestion>? suggestions)
    {
        if (string.IsNullOrEmpty(bettingStrategyName))
        {
            throw new ArgumentOutOfRangeException(nameof(bettingStrategyName), 
                "bettingStrategyName cannot be null or empty");
        }

        if (suggestions is null)
        {
            throw new ArgumentNullException(nameof(suggestions));
        }
            
        EnsureDirectoryExists(_outputPath);

        var filePath = Path.Combine(_outputPath, $"{bettingStrategyName}_suggestions_{DateUtility.GetTimestamp()}.csv");

        await WriteFileAsync(filePath, suggestions);
    }

    private  async Task WriteStatisticsAsync(string bettingStrategyName, Statistics? statistics)
    {
        if (string.IsNullOrEmpty(bettingStrategyName))
        {
            throw new ArgumentOutOfRangeException(nameof(bettingStrategyName), 
                "bettingStrategyName cannot be null or empty");
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