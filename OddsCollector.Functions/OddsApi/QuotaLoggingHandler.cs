using System.Globalization;
using System.Text;
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
        var builder = new StringBuilder("Odds API credits: ");

        if (TryGetCredits(response, RemainingHeader, out var remaining))
        {
            builder.Append($"{remaining} remaining");
        }

        if (TryGetCredits(response, UsedHeader, out var used))
        {
            builder.Append($", {used} used");
        }

        if (TryGetCredits(response, LastCallHeader, out var lastCall))
        {
            builder.Append($", {lastCall} spent on the last call");
        }

#pragma warning disable CA2254
#pragma warning disable CA1873
        logger.LogInformation(builder.ToString());
#pragma warning restore CA1873
#pragma warning restore CA2254
    }

    private static bool TryGetCredits(HttpResponseMessage response, string name, out int credits)
    {
        credits = 0;

        return response.Headers.TryGetValues(name, out var values) &&
               int.TryParse(values.FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out credits);
    }
}
