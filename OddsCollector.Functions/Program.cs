using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Hosting;

[assembly: InternalsVisibleTo("OddsCollector.Functions.Tests")]
// DynamicProxyGenAssembly2 is a temporary assembly built by mocking systems that use CastleProxy
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace OddsCollector.Functions;

[ExcludeFromCodeCoverage]
internal static class Program
{
    private static void Main()
    {
        HostProvider.Get().Run();
    }
}
