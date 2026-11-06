using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.Predictions;
using OddsCollector.Functions.Predictions.Configuration;

namespace OddsCollector.Functions.UnitTests.Tests.Predictions.Configuration;

internal sealed class ServiceCollectionExtensionsTests
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
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IPredictionStrategy>();
        var second = provider.GetRequiredService<IPredictionStrategy>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<PredictionStrategy>();
        second.Should().BeSameAs(first);
    }

    [Test]
    public void AddPredictionStrategy_ResolvingOutcomePredictor_ReturnsSingletonInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IOutcomePredictor>();
        var second = provider.GetRequiredService<IOutcomePredictor>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<OutcomePredictor>();
        second.Should().BeSameAs(first);
    }

    [Test]
    public void AddPredictionStrategy_ResolvingScoreCalculator_ReturnsSingletonInstance()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var first = provider.GetRequiredService<IScoreCalculator>();
        var second = provider.GetRequiredService<IScoreCalculator>();

        // Assert
        using var scope = new AssertionScope();

        first.Should().BeOfType<ScoreCalculator>();
        second.Should().BeSameAs(first);
    }

    [Test]
    public void AddPredictionStrategy_ResolvingTimeProvider_ReturnsSystemTimeProvider()
    {
        // Arrange
        using var provider = BuildProvider();

        // Act
        var timeProvider = provider.GetRequiredService<TimeProvider>();

        // Assert
        timeProvider.Should().BeSameAs(TimeProvider.System);
    }
}
