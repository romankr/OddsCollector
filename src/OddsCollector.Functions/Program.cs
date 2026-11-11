using System.Diagnostics.CodeAnalysis;

namespace OddsCollector.Functions;

internal static class Program
{
    [ExcludeFromCodeCoverage]
    private static void Main(string[] args)
    {
        HostProvider.CreateHost(args).Run();
    }
}
