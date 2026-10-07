using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.Predictions.Configuration;
using FunctionApp = OddsCollector.Functions.Predictions;

namespace OddsCollector.Functions.Tests.Tests.Predictions.Configuration;

internal sealed class ServiceCollectionExtensions
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddPredictionStrategy();

        return services.BuildServiceProvider();
    }

    [Test]
    public void AddPredictionStrategy_ResolvingPredictionStrategy_ReturnsSingletonInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.IPredictionStrategy>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<FunctionApp.PredictionStrategy>();
        provider.GetRequiredService<FunctionApp.IPredictionStrategy>().Should().BeSameAs(first);
    }

    [Test]
    public void AddPredictionStrategy_ResolvingOutcomePredictor_ReturnsSingletonInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.IOutcomePredictor>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<FunctionApp.OutcomePredictor>();
        provider.GetRequiredService<FunctionApp.IOutcomePredictor>().Should().BeSameAs(first);
    }

    [Test]
    public void AddPredictionStrategy_ResolvingScoreCalculator_ReturnsSingletonInstance()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.IScoreCalculator>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<FunctionApp.ScoreCalculator>();
        provider.GetRequiredService<FunctionApp.IScoreCalculator>().Should().BeSameAs(first);
    }

    [Test]
    public void AddPredictionStrategy_ResolvingTimeProvider_ReturnsSystemTimeProvider()
    {
        using var provider = BuildProvider();

        provider.GetRequiredService<TimeProvider>().Should().BeSameAs(TimeProvider.System);
    }
}
