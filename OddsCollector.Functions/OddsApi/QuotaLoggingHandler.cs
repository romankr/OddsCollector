using System.Globalization;
using Microsoft.Extensions.Logging;

namespace OddsCollector.Functions.OddsApi;

/// <summary>
///     Logs the credits The Odds API reports on every response.
/// </summary>
/// <remarks>
///     Each count is a separate numeric field of the log record, so an alert can query it directly, for example
///     <c>customDimensions.RemainingCredits</c> in Application Insights. A count whose header is missing or not
///     an integer is null.
/// </remarks>
internal sealed partial class QuotaLoggingHandler(ILogger<QuotaLoggingHandler> logger) : DelegatingHandler
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
        if (!logger.IsEnabled(LogLevel.Information))
        {
            return;
        }

        var remaining = GetCredits(response, RemainingHeader);
        var used = GetCredits(response, UsedHeader);
        var lastCall = GetCredits(response, LastCallHeader);

        if (remaining is null && used is null && lastCall is null)
        {
            return;
        }

        LogCredits(logger, remaining, used, lastCall);
    }

    private static int? GetCredits(HttpResponseMessage response, string name)
    {
        return response.Headers.TryGetValues(name, out var values) &&
               int.TryParse(values.FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                   out var credits)
            ? credits
            : null;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Odds API credits: {RemainingCredits} remaining, {UsedCredits} used, " +
                  "{LastCallCredits} spent on the last call")]
    private static partial void LogCredits(ILogger logger, int? remainingCredits, int? usedCredits,
        int? lastCallCredits);
}
