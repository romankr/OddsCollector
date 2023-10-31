using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace OddsCollector.Common.ServiceBus;

public static class DefaultHandlers
{
    public static Task ErrorHandler(ILogger logger, ProcessErrorEventArgs arguments)
    {
        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (arguments is null)
        {
            throw new ArgumentNullException(nameof(arguments));
        }

        logger.LogError(arguments.Exception, "Failed to process service bus message");

        return Task.CompletedTask;
    }
}
