namespace OddsCollector.Functions.Tests.Infrastructure.CancellationToken;

internal static class CancellationTokenGenerator
{
    public static async Task<System.Threading.CancellationToken> GetRequestedForCancellationToken()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        return cancellationTokenSource.Token;
    }
}
