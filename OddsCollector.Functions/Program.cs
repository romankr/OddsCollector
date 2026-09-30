using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;

[assembly: InternalsVisibleTo("OddsCollector.Functions.Tests")]
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
