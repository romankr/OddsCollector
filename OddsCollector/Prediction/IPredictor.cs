namespace OddsCollector.Prediction;

using Models;

public interface IPredictor
{
    IEnumerable<Prediction> GetPredictions(IEnumerable<SportEvent> events);

    Statistics GetStatistics(IEnumerable<Prediction> predictions);
}