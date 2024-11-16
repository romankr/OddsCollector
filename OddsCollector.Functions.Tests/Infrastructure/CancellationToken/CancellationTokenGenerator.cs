namespace OddsCollector.Functions.Tests.Infrastructure.CancellationToken;

internal static class CancellationTokenGenerator
{
    public static async Task<System.Threading.CancellationToken> GetRequestedForCancellationToken()
    {
        var source = new CancellationTokenSource();

        await source.CancelAsync();

        return source.Token;
    }
}
