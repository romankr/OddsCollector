using System.Net;
using System.Net.Sockets;

namespace OddsCollector.Functions.IntegrationTests.Infrastructure.Network;

internal static class FreePort
{
    public static int Get(params int[] excluded)
    {
        while (true)
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            if (!excluded.Contains(port))
            {
                return port;
            }
        }
    }
}
