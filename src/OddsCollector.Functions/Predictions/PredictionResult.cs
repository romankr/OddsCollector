using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal readonly union PredictionResult(EventPrediction, NoUsableOdds);
