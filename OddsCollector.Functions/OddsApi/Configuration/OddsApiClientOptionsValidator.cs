using Microsoft.Extensions.Options;

namespace OddsCollector.Functions.OddsApi.Configuration;

internal sealed class OddsApiClientOptionsValidator : IValidateOptions<OddsApiClientOptions>
{
    public ValidateOptionsResult Validate(string? name, OddsApiClientOptions options)
    {
        List<string> failures = [];

        if (options.Leagues.Count == 0)
        {
            failures.Add($"{OddsApiClientOptions.LeaguesKey} must name at least one league, " +
                         "for example soccer_epl;soccer_spain_la_liga");
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            failures.Add($"{OddsApiClientOptions.ApiKeyKey} is required");
        }

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUrl) ||
            (baseUrl.Scheme != Uri.UriSchemeHttp && baseUrl.Scheme != Uri.UriSchemeHttps))
        {
            failures.Add(
                $"{OddsApiClientOptions.BaseUrlKey} must be an absolute HTTP(S) URL. Actual value: {options.BaseUrl}");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
