namespace OddsCollector.Csv
{
    using Prediction;
    
    public interface ICsvSaver
    {
        void WritePredictions(string? dir, IEnumerable<Prediction>? predictions);

        void WriteStatistics(string? dir, Statistics? statistics);
    }
}
