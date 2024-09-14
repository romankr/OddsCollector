using Microsoft.Extensions.Logging;

namespace OddsCollector.Functions.Tests.Infrastructure.Logger;

internal static class LoggerFactory
{
    public static ILogger<T> GetLoggerMock<T>() where T : class
    {
        return Substitute.For<ILogger<T>>();
    }
}
