using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.Predictions.Configuration;
using FunctionApp = OddsCollector.Functions.Predictions;

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
                x => x.ServiceType == typeof(FunctionApp.IPredictionStrategy)
                     && x.ImplementationType == typeof(FunctionApp.PredictionStrategy)
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
                x => x.ServiceType == typeof(FunctionApp.IWinnerFinder)
                     && x.ImplementationType == typeof(FunctionApp.WinnerFinder)
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
                x => x.ServiceType == typeof(FunctionApp.IScoreCalculator)
                     && x.ImplementationType == typeof(FunctionApp.ScoreCalculator)
                     && x.Lifetime == ServiceLifetime.Singleton);

        strategyDescriptor.Should().NotBeNull();
    }
}
