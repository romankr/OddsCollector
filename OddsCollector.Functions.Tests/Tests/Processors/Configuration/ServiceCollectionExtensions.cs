using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using OddsCollector.Functions.Processors.Configuration;
using FunctionApp = OddsCollector.Functions.Processors;

namespace OddsCollector.Functions.Tests.Tests.Processors.Configuration;

internal sealed class ServiceCollectionExtensions
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddFunctionProcessors();

        return services.BuildServiceProvider();
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
