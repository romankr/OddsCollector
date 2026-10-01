namespace OddsCollector.Functions.IntegrationTests.Infrastructure.Polling;

/// <summary>
///     Waits for an asynchronous side effect, such as a message processed by a trigger, to become visible.
/// </summary>
internal static class Eventually
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

    public static async Task<T> GetAsync<T>(Func<CancellationToken, Task<T?>> probe, TimeSpan timeout,
        string description, CancellationToken cancellationToken = default) where T : class
    {
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);

        Exception? lastException = null;

        while (true)
        {
            try
            {
                var result = await probe(timeoutSource.Token);

                if (result is not null)
                {
                    return result;
                }
            }
            catch (Exception exception) when (!timeoutSource.IsCancellationRequested)
            {
                lastException = exception;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // The timeout fired while the probe was running; reported below.
            }

            try
            {
                await Task.Delay(Interval, timeoutSource.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException($"Timed out after {timeout} waiting for {description}", lastException);
            }
        }
    }
}
