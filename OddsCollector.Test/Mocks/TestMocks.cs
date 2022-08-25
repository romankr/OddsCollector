namespace OddsCollector.Test.Mocks;

using Microsoft.Extensions.Logging;
using Moq;

public static class TestMocks
{
    public static Mock<ILogger<T>> GetLoggerMock<T>()
    {
        return new Mock<ILogger<T>>();
    }
}