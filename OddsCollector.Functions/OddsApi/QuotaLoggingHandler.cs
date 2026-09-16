using System.Globalization;
using Microsoft.Extensions.Logging;

namespace OddsCollector.Functions.OddsApi;

internal sealed class QuotaLoggingHandler(ILogger<QuotaLoggingHandler> logger) : DelegatingHandler
{
    internal const string RemainingHeader = "x-requests-remaining";
    internal const string UsedHeader = "x-requests-used";
    internal const string LastCallHeader = "x-requests-last";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        LogQuota(response);

        return response;
    }

    private void LogQuota(HttpResponseMessage response)
    {
        // Reading the headers is only worth the work when the entry is going to be written.
        if (!logger.IsEnabled(LogLevel.Information))
        {
            return;
        }

        List<string> credits = [];

        if (TryGetCredits(response, RemainingHeader, out var remaining))
        {
            credits.Add($"{remaining} remaining");
        }

        if (TryGetCredits(response, UsedHeader, out var used))
        {
            credits.Add($"{used} used");
        }

        if (TryGetCredits(response, LastCallHeader, out var lastCall))
        {
            credits.Add($"{lastCall} spent on the last call");
        }

        // A response carrying no usable quota header says nothing worth an entry.
        if (credits.Count == 0)
        {
            return;
        }

        logger.LogInformation("Odds API credits: {Credits}", string.Join(", ", credits));
    }

    private static bool TryGetCredits(HttpResponseMessage response, string name, out int credits)
    {
        credits = 0;

        return response.Headers.TryGetValues(name, out var values) &&
               int.TryParse(values.FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out credits);
    }
}
