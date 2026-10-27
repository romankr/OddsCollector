using System.Diagnostics.CodeAnalysis;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IPredictionStrategy
{
    bool TryGetPrediction(UpcomingEvent upcomingEvent, [NotNullWhen(true)] out EventPrediction? prediction);
}
