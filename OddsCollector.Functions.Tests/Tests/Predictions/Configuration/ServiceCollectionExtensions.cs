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
    public void AddPredictionStrategy_ResolvesPredictionStrategy()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.IPredictionStrategy>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<FunctionApp.PredictionStrategy>();
        provider.GetRequiredService<FunctionApp.IPredictionStrategy>().Should().BeSameAs(first);
    }

    [Test]
    public void AddPredictionStrategy_ResolvesWinnerFinder()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.IWinnerFinder>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<FunctionApp.WinnerFinder>();
        provider.GetRequiredService<FunctionApp.IWinnerFinder>().Should().BeSameAs(first);
    }

    [Test]
    public void AddPredictionStrategy_ResolvesScoreCalculator()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.IScoreCalculator>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<FunctionApp.ScoreCalculator>();
        provider.GetRequiredService<FunctionApp.IScoreCalculator>().Should().BeSameAs(first);
    }
}
