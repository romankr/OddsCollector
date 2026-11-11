using System.Diagnostics.CodeAnalysis;

namespace OddsCollector.Functions.Predictions;

// No outcome of the event scores above zero, for example because every bookmaker quoted unusable prices.
// It is a normal result, not a failure, so it is returned instead of thrown.
[SuppressMessage("Major Code Smell", "S2094:Classes should not be empty",
    Justification = "A case of OutcomeResult and PredictionResult: the type itself is the information.")]
internal sealed record NoUsableOdds;
