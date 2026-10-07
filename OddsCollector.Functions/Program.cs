using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;

[assembly: InternalsVisibleTo("OddsCollector.Functions.UnitTests")]
[assembly: InternalsVisibleTo("OddsCollector.Functions.IntegrationTests")]
[assembly: InternalsVisibleTo("OddsCollector.Tests.Infrastructure")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace OddsCollector.Functions;

internal static class Program
{
    [ExcludeFromCodeCoverage]
    private static void Main()
    {
        HostProvider.Get().Run();
    }
}
