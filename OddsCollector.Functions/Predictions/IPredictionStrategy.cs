using System.Diagnostics.CodeAnalysis;
using OddsCollector.Functions.Models;

namespace OddsCollector.Functions.Predictions;

internal interface IPredictionStrategy
{
    /// <returns><see langword="false" /> when the event's odds are not usable, so nothing can be predicted.</returns>
    bool TryGetPrediction(UpcomingEvent upcomingEvent, [NotNullWhen(true)] out EventPrediction? prediction);
}
