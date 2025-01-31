using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.Predictions;
using OddsCollector.Functions.Predictions.Configuration;

namespace OddsCollector.Functions.Tests.Tests.Predictions.Configuration;

internal sealed class ServiceCollectionExtensions
{
    [Test]
    public void AddPredictionStrategy_AddsPredictionStrategy()
    {
        var services = new ServiceCollection();

        services.AddPredictionStrategy();

        var descriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IPredictionStrategy)
                     && x.ImplementationType == typeof(OddsCollector.Functions.Predictions.PredictionStrategy)
                     && x.Lifetime == ServiceLifetime.Singleton);

        descriptor.Should().NotBeNull();
    }

    [Test]
    public void AddPredictionStrategy_AddsWinnerFinder()
    {
        var services = new ServiceCollection();

        services.AddPredictionStrategy();

        var strategyDescriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IWinnerFinder)
                     && x.ImplementationType == typeof(OddsCollector.Functions.Predictions.WinnerFinder)
                     && x.Lifetime == ServiceLifetime.Singleton);

        strategyDescriptor.Should().NotBeNull();
    }

    [Test]
    public void AddPredictionStrategy_AddsScoreCalculator()
    {
        var services = new ServiceCollection();

        services.AddPredictionStrategy();

        var strategyDescriptor =
            services.FirstOrDefault(
                x => x.ServiceType == typeof(IScoreCalculator)
                     && x.ImplementationType == typeof(OddsCollector.Functions.Predictions.ScoreCalculator)
                     && x.Lifetime == ServiceLifetime.Singleton);

        strategyDescriptor.Should().NotBeNull();
    }
}
