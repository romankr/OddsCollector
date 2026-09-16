using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.Predictions.Configuration;
using OddsCollector.Functions.Processors.Configuration;
using FunctionApp = OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Tests.Tests.Processors.Configuration;

internal sealed class ServiceCollectionExtensions
{
    // PredictionProcessor takes an IPredictionStrategy, which AddFunctionProcessors does not
    // register, so the provider composes both extensions the way HostProvider does.
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddPredictionStrategy();
        services.AddFunctionProcessors();

        return services.BuildServiceProvider();
    }

    [Test]
    public void AddFunctionProcessors_ResolvesPredictionProcessor()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.IPredictionProcessor>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<FunctionApp.PredictionProcessor>();
        provider.GetRequiredService<FunctionApp.IPredictionProcessor>().Should().BeSameAs(first);
    }

    [Test]
    public void AddFunctionProcessors_ResolvesPredictionHttpRequestProcessor()
    {
        using var provider = BuildProvider();

        var first = provider.GetRequiredService<FunctionApp.IPredictionHttpRequestProcessor>();

        using var scope = new AssertionScope();

        first.Should().BeOfType<FunctionApp.PredictionHttpRequestProcessor>();
        provider.GetRequiredService<FunctionApp.IPredictionHttpRequestProcessor>().Should().BeSameAs(first);
    }
}
