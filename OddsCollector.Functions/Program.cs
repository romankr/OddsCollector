using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;

namespace OddsCollector.Functions;

internal static class Program
{
    [ExcludeFromCodeCoverage]
    private static void Main()
    {
        HostProvider.Get().Run();
    }
}
