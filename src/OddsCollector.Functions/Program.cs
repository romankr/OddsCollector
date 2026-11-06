using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;

namespace OddsCollector.Functions;

internal static class Program
{
    [ExcludeFromCodeCoverage]
    private static void Main(string[] args)
    {
        HostProvider.CreateHost(args).Run();
    }
}
