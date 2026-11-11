using Azure.Core.Serialization;
using OddsCollector.Functions.Functions;

namespace OddsCollector.Functions.UnitTests.Infrastructure.Http;

// Serializes like the worker, but throws the given exception for the predictions response, so that a failure
// while writing the response can be tested.
internal sealed class PredictionsThrowingSerializer(Exception exception) : ObjectSerializer
{
    private readonly JsonObjectSerializer _inner = new();

    public override void Serialize(Stream stream, object? value, Type inputType,
        CancellationToken cancellationToken)
    {
        ThrowIfPredictions(value);
        _inner.Serialize(stream, value, inputType, cancellationToken);
    }

    public override ValueTask SerializeAsync(Stream stream, object? value, Type inputType,
        CancellationToken cancellationToken)
    {
        ThrowIfPredictions(value);
        return _inner.SerializeAsync(stream, value, inputType, cancellationToken);
    }

    public override object? Deserialize(Stream stream, Type returnType, CancellationToken cancellationToken)
    {
        return _inner.Deserialize(stream, returnType, cancellationToken);
    }

    public override ValueTask<object?> DeserializeAsync(Stream stream, Type returnType,
        CancellationToken cancellationToken)
    {
        return _inner.DeserializeAsync(stream, returnType, cancellationToken);
    }

    private void ThrowIfPredictions(object? value)
    {
        if (value is EventPredictionResponse[])
        {
            throw exception;
        }
    }
}
