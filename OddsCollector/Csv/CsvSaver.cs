namespace OddsCollector.Csv
{
    using CsvHelper;
    using Prediction;
    using System.Globalization;

    public class CsvSaver : ICsvSaver
    {
        private static string GetCurrentTimestamp()
        {
            return DateTime.Now.ToString("yyyyddMMHHmmssffff");
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

        public void WritePredictions(string? dir, IEnumerable<Prediction>? predictions)
        {
            if (string.IsNullOrEmpty(dir))
            {
                throw new ArgumentOutOfRangeException(nameof(dir), "dir cannot be null or empty");
            }

            if (predictions is null)
            {
                throw new ArgumentNullException(nameof(predictions));
            }
            
            EnsureDirectoryExists(dir);

            var filePath = Path.Combine(dir, $"predictions_{GetCurrentTimestamp()}.csv");

            WriteFile(filePath, predictions);
        }

        public void WriteStatistics(string? dir, Statistics? statistics)
        {
            if (string.IsNullOrEmpty(dir))
            {
                throw new ArgumentOutOfRangeException(nameof(dir), "dir cannot be null or empty");
            }

            if (statistics is null)
            {
                throw new ArgumentNullException(nameof(statistics));
            }

            EnsureDirectoryExists(dir);

            var filePath = Path.Combine(dir, $"statistics_{GetCurrentTimestamp()}.csv");

            WriteFile(filePath, new List<Statistics> { statistics });
        }
    }
}