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

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<FunctionApp.IPredictionStrategy>();
        service.Should().NotBeNull();
    }

    [Test]
    public void AddPredictionStrategy_AddsOutcomeFinder()
    {
        var services = new ServiceCollection();

        services.AddPredictionStrategy();

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<FunctionApp.IOutcomeFinder>();
        service.Should().NotBeNull();
    }

    [Test]
    public void AddPredictionStrategy_AddsScoreCalculator()
    {
        var services = new ServiceCollection();

        services.AddPredictionStrategy();

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<FunctionApp.IScoreCalculator>();
        service.Should().NotBeNull();
    }
}
